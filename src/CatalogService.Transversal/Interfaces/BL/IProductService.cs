using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters;
using CatalogService.Transversal.Interfaces.Base;

namespace CatalogService.Transversal.Interfaces.BL
{
    public interface IProductService : ICrudBase<ProductDTO>, IGetPagedList<ProductDTO, ProductFilterParams>
    {
    }
}
