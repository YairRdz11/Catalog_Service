using Common.Utilities.Classes.Messaging.Events;

namespace CatalogService.Transversal.Classes.Events
{
    public sealed record ProductUpdatedEvent(Guid ProductId, string Name, decimal Price, Guid CategoryId)
    : IntegrationEventBase(nameof(ProductUpdatedEvent), 1);
}
