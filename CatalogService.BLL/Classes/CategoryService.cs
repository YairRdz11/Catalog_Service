using CatalogService.BLL.Classes.Validators;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Exceptions;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using System.Xml.Linq;
using Utilities.Classes.Common;

namespace CatalogService.BLL.Classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<CategoryDTO> CreateAsync(CategoryDTO entity)
        {
            entity.Id = Guid.NewGuid();
            await ValidateCategory(entity);

            if (entity.ParentCategoryId != Guid.Empty)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(entity.ParentCategoryId);
            }
            var result = await _categoryRepository.CreateAsync(entity);

            return result;
        }

        public async Task<CategoryDTO> DeleteAsync(Guid id)
        {
            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<CategoryDTO> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            var productsByCategory = await _productRepository.GetProductsByCategoryAsync(id);

            category.Products = productsByCategory;

            return category;
        }

        public async Task<IEnumerable<CategoryDTO>> GetListAsync()
        {
            return await _categoryRepository.GetListAsync();
        }

        public async Task<CategoryDTO> UpdateAsync(CategoryDTO categoryDTO)
        {
            var entity = await _categoryRepository.GetByIdAsync(categoryDTO.Id);
            ValidateCategory(categoryDTO);
            if (categoryDTO.ParentCategoryId != Guid.Empty)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(categoryDTO.ParentCategoryId);
            }
            var result = await _categoryRepository.UpdateAsync(categoryDTO);
            return result;
        }

        private async Task ValidateCategory(CategoryDTO category)
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

            if (await _categoryRepository.DoesItemExistByNameAsync(category.Name))
            {
                var errorList = new List<RuleError>
                {
                    new RuleError
                    {
                        ErrorMessage = $"Category with name '{category.Name}' already exists.",
                        PropertyName = "Name",
                        AttempedValue = category.Name,
                        MessageOrigin = "CategoryService"
                    }
                };
                throw new ValidateException(errorList);
            }
        }
    }
}
