using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.Base;

namespace CatalogService.Transversal.Interfaces.DAL
{
    public interface ICategoryRepository : ICrudBase<CategoryDTO>, IGetByNameAsync
    {
    }
}
