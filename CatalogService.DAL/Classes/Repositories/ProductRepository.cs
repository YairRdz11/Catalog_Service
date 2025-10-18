using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.DAL;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.DAL.Classes.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogBDContext _context;
        private readonly IMapper _mapper;

        public ProductRepository(CatalogBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            var products = await _context.Products.ToListAsync();
            
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public Task<ProductDTO> UpdateAsync(ProductDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
