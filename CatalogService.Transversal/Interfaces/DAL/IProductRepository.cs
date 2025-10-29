using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters;
using CatalogService.Transversal.Interfaces.Base;

namespace CatalogService.Transversal.Interfaces.DAL
{
    public interface IProductRepository : ICrudBase<ProductDTO>, IGetPagedList<ProductDTO, ProductFilterParams>, IGetByNameAsync
    {
    }
}
