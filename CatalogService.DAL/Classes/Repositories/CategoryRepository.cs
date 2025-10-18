using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Exceptions;
using CatalogService.Transversal.Interfaces.DAL;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace CatalogService.DAL.Classes.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogBDContext _context;
        private readonly IMapper _mapper;

        public CategoryRepository(CatalogBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Category> GetCategoryById(Guid id)
        {
            var entity = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                throw new NotFoundException("Category", id);
            }

            return entity;
        }

        public async Task<CategoryDTO> CreateAsync(CategoryDTO categoryDTO)
        {
            var entity = _mapper.Map<Category>(categoryDTO);
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<CategoryDTO> DeleteAsync(Guid id)
        {
            var entity = await GetCategoryById(id);
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
            
            var categoryDTO = _mapper.Map<CategoryDTO>(entity);
            return categoryDTO;
        }

        public async Task<CategoryDTO> GetByIdAsync(Guid id)
        {
            var entity = await GetCategoryById(id);

            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<IEnumerable<CategoryDTO>> GetListAsync()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> UpdateAsync(CategoryDTO entity)
        {
            var entityToUpdate = await GetCategoryById(entity.CategoryId);

            entityToUpdate.Name = entity.Name;
            entityToUpdate.Description = entity.Description;
            entityToUpdate.URL = entity.URL;
            entityToUpdate.ParentCategoryId = entity.ParentCategoryId != Guid.Empty ? entity.ParentCategoryId : null;
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDTO>(entityToUpdate);
        }
    }
}
