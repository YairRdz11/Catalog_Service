using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.Base;

namespace CatalogService.Transversal.Interfaces.BL
{
    public interface ICategoryService : ICrudBase<CategoryDTO>, IGetList<CategoryDTO>
    {
    }
}
