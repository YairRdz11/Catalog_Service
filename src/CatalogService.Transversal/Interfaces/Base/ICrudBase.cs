using Common.Utilities.Classes.Common;

namespace CatalogService.Transversal.Interfaces.Base
{
    public interface ICrudBase<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> GetByIdAsync(Guid id);
        Task<T> DeleteAsync(Guid id);
    }
}
