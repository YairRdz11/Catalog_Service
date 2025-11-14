using Common.Utilities.Classes.Messaging.Events;

namespace CatalogService.Transversal.Classes.Events
{
    public sealed record ProductDeletedEvent(Guid ProductId)
    : IntegrationEventBase(nameof(ProductDeletedEvent), 1);
}
