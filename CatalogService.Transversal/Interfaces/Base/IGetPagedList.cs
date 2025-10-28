using Common.Utilities.Classes.Common;

namespace CatalogService.Transversal.Interfaces.Base
{
    public interface IGetPagedList<T>
    {
        Task<IEnumerable<T>> GetListAsync(PaginationParams paginationParams);
    }
}
