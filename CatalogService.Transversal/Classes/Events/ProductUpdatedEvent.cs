namespace CatalogService.Transversal.Classes.Events
{
    public sealed record ProductUpdatedEvent(Guid ProductId, string Name, decimal Price, Guid CategoryId, string? CategoryName)
    : IntegrationEventBase(nameof(ProductUpdatedEvent), 1);
}
