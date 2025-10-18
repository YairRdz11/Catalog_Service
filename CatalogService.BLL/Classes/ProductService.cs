using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;

namespace CatalogService.BLL.Classes
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public Task<ProductDTO> CreateAsync(ProductDTO entity)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDTO> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDTO> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProductDTO>> GetListAsync()
        {
            return await _repository.GetListAsync();
        }

        public Task<ProductDTO> UpdateAsync(ProductDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
