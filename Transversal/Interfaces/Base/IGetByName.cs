namespace CatalogService.Transversal.Interfaces.Base
{
    public interface IGetByNameAsync
    {
        Task<bool> DoesItemExistByNameAsync(string name);
    }
}
