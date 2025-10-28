namespace CatalogService.Transversal.Interfaces.Base
{
    public interface IGetList<T>
    {
        Task<IEnumerable<T>> GetListAsync();
    }
}
