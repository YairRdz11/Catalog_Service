using Common.Utilities.Classes.Common;

namespace CatalogService.Transversal.Interfaces.Base
{
    public interface IGetPagedList<T, TParams>
    {
        Task<IEnumerable<T>> GetListAsync(TParams paginationParams);
    }
}
