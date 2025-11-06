namespace CatalogService.Transversal.Interfaces.Events
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredOnUtc { get; }
        string EventType { get; }
        int Version { get; }
    }
}
