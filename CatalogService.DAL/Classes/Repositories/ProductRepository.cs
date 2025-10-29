using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters;
using CatalogService.Transversal.Interfaces.DAL;
using Common.Utilities.Classes.Exceptions;
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

        public async Task<ProductDTO> DeleteAsync(Guid id)
        {
            var entity = await GetProductById(id);
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            var productDTO = _mapper.Map<ProductDTO>(entity);
            return productDTO;
        }

        public async Task<ProductDTO> GetByIdAsync(Guid id)
        {
            var entity = await GetProductById(id);

            return _mapper.Map<ProductDTO>(entity);
        }

        public async Task<ProductDTO> UpdateAsync(ProductDTO entity)
        {
            var productEntity = await GetProductById(entity.Id);

            productEntity.Name = entity.Name;
            productEntity.Description = entity.Description;
            productEntity.ImageUrl = entity.URL;
            productEntity.Price = entity.Price;
            productEntity.Amount = entity.Amount;
            productEntity.CategoryId = entity.CategoryId;
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDTO>(productEntity);
        }

        public async Task<bool> DoesItemExistByNameAsync(string name)
        {
            var normalized = name.Trim().ToUpperInvariant();

            var entity = await _context.Products.FirstOrDefaultAsync(c => c.Name.ToUpper() == normalized);

            return entity != null;
        }

        public async Task<IEnumerable<ProductDTO>> GetListAsync(ProductFilterParams filter)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);
            var query = _context.Products.AsQueryable();

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            query = query.OrderBy(p => p.Name)
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize);

            var products = await query.ToListAsync();

            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }
    }
}
