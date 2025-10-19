namespace CatalogService.Transversal.Classes.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key)
            : base($"The entity '{entityName}' with key '{key}' was not found.")
        {
        }
    }
}
