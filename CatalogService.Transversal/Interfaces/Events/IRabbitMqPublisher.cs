namespace CatalogService.Transversal.Interfaces.Events
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(IIntegrationEvent @event, string routingKey, CancellationToken ct = default);
    }
}
