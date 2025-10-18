using CatalogService.BLL.Classes.Validators;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Exceptions;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using Utilities.Classes.Common;

namespace CatalogService.BLL.Classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        } 

        public async Task<CategoryDTO> CreateAsync(CategoryDTO entity)
        {
            entity.CategoryId = Guid.NewGuid();
            ValidateCategory(entity);

            if (entity.ParentCategoryId != Guid.Empty)
            {
                var parentCategory = await _repository.GetByIdAsync(entity.ParentCategoryId);
            }
            var result = await _repository.CreateAsync(entity);
            
            return result;
        }

        public async Task<CategoryDTO> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<CategoryDTO> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<CategoryDTO>> GetListAsync()
        {
            return await _repository.GetListAsync();
        }

        public async Task<CategoryDTO> UpdateAsync(CategoryDTO categoryDTO)
        {
            var entity = await _repository.GetByIdAsync(categoryDTO.CategoryId);
            ValidateCategory(categoryDTO);
            if (categoryDTO.ParentCategoryId != Guid.Empty)
            {
                var parentCategory = await _repository.GetByIdAsync(categoryDTO.ParentCategoryId);
            }
            var result = await _repository.UpdateAsync(categoryDTO);
            return result;
        }

        private void ValidateCategory(CategoryDTO category)
        {
            var validator = new CategoryValidator();
            validator.CategoryValidate();
            var validationResult = validator.Validate(category);
            if (!validationResult.IsValid)
            {
                var errorList = validationResult.Errors.Select(e => new RuleError
                {
                    ErrorMessage = e.ErrorMessage,
                    PropertyName = e.PropertyName,
                    AttempedValue = e.AttemptedValue?.ToString(),
                    MessageOrigin = "CategoryValidator"
                }).ToList();

                throw new ValidateException(errorList);
            }
        }
    }
}
