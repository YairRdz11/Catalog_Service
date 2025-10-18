using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Exceptions;
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

        private async Task<Product> GetProductById(Guid id)
        {
            var entity = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
            {
                throw new NotFoundException($"Product", id);
            }
            return entity;
        }

        public async Task<ProductDTO> CreateAsync(ProductDTO productDTO)
        {
            var entity = _mapper.Map<Product>(productDTO);
            await _context.Products.AddAsync(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDTO>(entity);
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
