using CatalogService.Transversal.Classes.Dtos;

namespace CatalogService.Transversal.Interfaces.Base
{
    public interface IGetProductsByCategory
    {
        Task<IEnumerable<ProductDTO>> GetProductsByCategory(Guid categoryId);
    }
}
