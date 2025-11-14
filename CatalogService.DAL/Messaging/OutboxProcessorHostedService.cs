using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.DAL.Classes.Data.Enums;
using CatalogService.Transversal.Interfaces.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace CatalogService.DAL.Messaging
{
    public class OutboxProcessorHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorHostedService> _logger;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);
        private readonly int _maxAttempts = 5;

        private readonly AsyncRetryPolicy _publishRetryPolicy;

        public OutboxProcessorHostedService(IServiceProvider serviceProvider,
                                            ILogger<OutboxProcessorHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _publishRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5)
                    },
                    (ex, ts, attempt, ctx) =>
                    {
                        _logger.LogWarning(ex, "Retry attempt {Attempt} after {Delay} for outbox publish failure.", attempt, ts);
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessorHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing outbox.");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("OutboxProcessorHostedService stopping.");
        }

        private async Task ProcessPendingEventsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogBDContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            // Fetch a batch of pending events
            var batch = await db.OutboxEvents
                .Where(x => x.Status == OutboxEventStatus.Pending || x.Status == OutboxEventStatus.Failed)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(50)
                .ToListAsync(ct);

            if (batch.Count == 0) return;

            foreach (var outboxEvent in batch)
            {
                if (ct.IsCancellationRequested) break;

                outboxEvent.Status = OutboxEventStatus.Processing;
                await db.SaveChangesAsync(ct);

                try
                {
                    var integrationEvent = Deserialize(outboxEvent);

                    await _publishRetryPolicy.ExecuteAsync(async () =>
                    {
                        await publisher.PublishAsync(integrationEvent, outboxEvent.RoutingKey, ct);
                    });

                    outboxEvent.Status = OutboxEventStatus.Succeeded;
                    outboxEvent.ProcessedOnUtc = DateTime.UtcNow;
                    outboxEvent.LastError = null;
                }
                catch (Exception ex)
                {
                    outboxEvent.Attempts += 1;
                    outboxEvent.LastError = ex.Message;

                    if (outboxEvent.Attempts >= _maxAttempts)
                    {
                        outboxEvent.Status = OutboxEventStatus.Abandoned;
                        _logger.LogError(ex,
                            "Abandoning outbox event {OutboxEventId} after {Attempts} attempts.",
                            outboxEvent.Id, outboxEvent.Attempts);
                    }
                    else
                    {
                        outboxEvent.Status = OutboxEventStatus.Failed;
                        _logger.LogWarning(ex,
                            "Failed publishing outbox event {OutboxEventId}. Attempts: {Attempts}", outboxEvent.Id, outboxEvent.Attempts);
                    }
                }

                await db.SaveChangesAsync(ct);
            }
        }

        private static IIntegrationEvent Deserialize(OutboxEvent outbox)
        {

            var type = Type.GetType(outbox.EventType);
            if (type == null)
                throw new InvalidOperationException($"Cannot resolve event type '{outbox.EventType}'.");

            return (IIntegrationEvent?)JsonSerializer.Deserialize(outbox.Payload, type)
                ?? throw new InvalidOperationException("Failed to deserialize integration event.");
        }
    }
}
