namespace CatalogService.Transversal.Classes.Events
{
    public sealed record CategoryUpdatedEvent(Guid CategoryId, string Name)
    : IntegrationEventBase(nameof(CategoryUpdatedEvent), 1);
}
