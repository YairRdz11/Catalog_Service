using CatalogService.Transversal.Interfaces.Events;

namespace CatalogService.Transversal.Classes.Events
{
    public abstract record IntegrationEventBase(string EventType, int Version) : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
        public string EventType => EventTypeField;
        public int Version => VersionField;
        private string EventTypeField => GetType().Name;
        private int VersionField => 1;
    }
}
