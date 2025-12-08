using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.DAL.Classes.Data.Enums;
using Common.Utilities.Interfaces.Messaging.Events;
using System.Text.Json;

namespace CatalogService.DAL.Messaging
{
    public class OutboxWriter : IOutboxWriter
    {
        private readonly CatalogBDContext _dbContext;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public OutboxWriter(CatalogBDContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(IIntegrationEvent @event, string routingKey, string? correlationId = null)
        {
            var outbox = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = @event.GetType().AssemblyQualifiedName!,
                OccurredOnUtc = @event.OccurredOnUtc,
                Payload = JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions),
                RoutingKey = routingKey,
                Version = @event.Version,
                Attempts = 0,
                Status = OutboxEventStatus.Pending,
                CorrelationId = correlationId
            };

            _dbContext.OutboxEvents.Add(outbox);
        }
    }
}
