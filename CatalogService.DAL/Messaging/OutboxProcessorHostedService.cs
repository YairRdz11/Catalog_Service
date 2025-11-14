using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.DAL.Classes.Data.Enums;
using CatalogService.Transversal.Classes.Events;
using Common.Utilities.Interfaces.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Common.Utilities.Classes.Exceptions;
using Common.Utilities.Classes.Messaging.Options;

namespace CatalogService.DAL.Messaging
{
    public class OutboxProcessorHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorHostedService> _logger;
        private readonly OutboxOptions _options;
        private AsyncRetryPolicy _publishRetryPolicy = null!;

        // Json options for deserialization (case-insensitive property names)
        private static readonly JsonSerializerOptions DeserializeOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        // Whitelist allowed event types
        private static readonly Dictionary<string, Type> AllowedEventTypes = new(StringComparer.Ordinal)
        {
            { typeof(CategoryUpdatedEvent).AssemblyQualifiedName!, typeof(CategoryUpdatedEvent) },
            { typeof(ProductUpdatedEvent).AssemblyQualifiedName!, typeof(ProductUpdatedEvent) },
            { typeof(ProductDeletedEvent).AssemblyQualifiedName!, typeof(ProductDeletedEvent) }
        };

        public OutboxProcessorHostedService(IServiceProvider serviceProvider,
                                            ILogger<OutboxProcessorHostedService> logger,
                                            IOptions<OutboxOptions> optionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = optionsAccessor.Value;
            BuildRetryPolicy();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessorHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await SafeProcessCycleAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("OutboxProcessorHostedService stopping.");
        }

        private async Task SafeProcessCycleAsync(CancellationToken ct)
        {
            try
            {
                await ProcessPendingEventsAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing outbox.");
            }
        }

        private void BuildRetryPolicy()
        {
            _publishRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)
                }, (ex, ts, attempt, ctx) =>
                {
                    _logger.LogWarning(ex, "Retry attempt {Attempt} after {Delay} for outbox publish failure.", attempt, ts);
                });
        }

        private async Task ProcessPendingEventsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogBDContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            var batch = await LoadBatchAsync(db, ct);
            if (batch.Count == 0) return;

            MarkBatchProcessing(batch);
            await db.SaveChangesAsync(ct);

            foreach (var outboxEvent in batch)
            {
                if (ct.IsCancellationRequested) break;
                await ProcessSingleEventAsync(db, publisher, outboxEvent, ct);
            }

            await db.SaveChangesAsync(ct);
        }

        private async Task<List<OutboxEvent>> LoadBatchAsync(CatalogBDContext db, CancellationToken ct)
        {
            return await db.OutboxEvents
                .Where(x => x.Status == OutboxEventStatus.Pending || x.Status == OutboxEventStatus.Failed)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(_options.BatchSize)
                .ToListAsync(ct);
        }

        private void MarkBatchProcessing(IEnumerable<OutboxEvent> batch)
        {
            foreach (var e in batch)
            {
                e.Status = OutboxEventStatus.Processing;
            }
        }

        private async Task ProcessSingleEventAsync(CatalogBDContext db, IRabbitMqPublisher publisher, OutboxEvent outboxEvent, CancellationToken ct)
        {
            try
            {
                if (!ValidatePayloadSize(outboxEvent))
                {
                    return;
                }

                var integrationEvent = Deserialize(outboxEvent);

                await _publishRetryPolicy.ExecuteAsync(async () =>
                {
                    await publisher.PublishAsync(integrationEvent, outboxEvent.RoutingKey, ct);
                });

                MarkSucceeded(outboxEvent);
            }
            catch (TypeResolutionException tex)
            {
                MarkAbandoned(outboxEvent, "TypeResolutionFailed");
                _logger.LogError(tex, "Outbox event {Id} abandoned: cannot resolve type.", outboxEvent.Id);
            }
            catch (Exception ex)
            {
                HandleFailure(outboxEvent, ex);
            }
        }

        private bool ValidatePayloadSize(OutboxEvent outboxEvent)
        {
            if (outboxEvent.Payload.Length <= _options.MaxPayloadBytes) return true;
            outboxEvent.Status = OutboxEventStatus.Abandoned;
            outboxEvent.LastError = "PayloadTooLarge";
            _logger.LogWarning("Outbox event {Id} abandoned: payload exceeds {Limit} bytes.", outboxEvent.Id, _options.MaxPayloadBytes);
            return false;
        }

        private void MarkSucceeded(OutboxEvent outboxEvent)
        {
            outboxEvent.Status = OutboxEventStatus.Succeeded;
            outboxEvent.ProcessedOnUtc = DateTime.UtcNow;
            outboxEvent.LastError = null;
        }

        private void HandleFailure(OutboxEvent outboxEvent, Exception ex)
        {
            outboxEvent.Attempts += 1;
            outboxEvent.LastError = ex.Message;

            if (outboxEvent.Attempts >= _options.MaxAttempts)
            {
                MarkAbandoned(outboxEvent, ex.GetType().Name);
                _logger.LogError(ex, "Abandoning outbox event {OutboxEventId} after {Attempts} attempts.", outboxEvent.Id, outboxEvent.Attempts);
            }
            else
            {
                outboxEvent.Status = OutboxEventStatus.Failed;
                _logger.LogWarning(ex, "Failed publishing outbox event {OutboxEventId}. Attempts: {Attempts}", outboxEvent.Id, outboxEvent.Attempts);
            }
        }

        private void MarkAbandoned(OutboxEvent outboxEvent, string reason)
        {
            outboxEvent.Status = OutboxEventStatus.Abandoned;
            outboxEvent.LastError = reason;
        }

        private IIntegrationEvent Deserialize(OutboxEvent outbox)
        {
            if (!AllowedEventTypes.TryGetValue(outbox.EventType, out var type))
            {
                throw new TypeResolutionException($"Event type not allowed: {outbox.EventType}");
            }

            var evt = (IIntegrationEvent?)JsonSerializer.Deserialize(outbox.Payload, type, DeserializeOptions)
                ?? throw new InvalidOperationException("Deserialization returned null.");
            return evt;
        }
    }
}
