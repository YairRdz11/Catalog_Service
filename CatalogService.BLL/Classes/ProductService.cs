using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using Common.Utilities.Classes.Common;
using Common.Utilities.Classes.Exceptions;

namespace CatalogService.BLL.Classes
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository repository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        public async Task<ProductDTO> CreateAsync(ProductDTO productDTO)
        {
            productDTO.Id = Guid.NewGuid();
            await ValidateProduct(productDTO);
            var category = await _categoryRepository.GetByIdAsync(productDTO.CategoryId);
            var result = await _repository.CreateAsync(productDTO);
            return result;
        }

        private async Task ValidateProduct(ProductDTO productDTO)
        {
            var validator = new Validators.ProductValidator();
            validator.ProductValidate();
            var validationResult = validator.Validate(productDTO);
            if (!validationResult.IsValid)
            {
                var errorList = validationResult.Errors.Select(e => new RuleError
                {
                    ErrorMessage = e.ErrorMessage,
                    PropertyName = e.PropertyName,
                    AttempedValue = e.AttemptedValue?.ToString(),
                    MessageOrigin = "ProductValidator"
                }).ToList();

                throw new ValidateException(errorList);
            }

            if (await _repository.DoesItemExistByNameAsync(productDTO.Name))
            {
                var errorList = new List<RuleError>
                {
                    new RuleError
                    {
                        ErrorMessage = $"Category with name '{productDTO.Name}' already exists.",
                        PropertyName = "Name",
                        AttempedValue = productDTO.Name,
                        MessageOrigin = "CategoryService"
                    }
                };
                throw new ValidateException(errorList);
            }
        }

        public async Task<ProductDTO> DeleteAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            return await _repository.DeleteAsync(id);
        }

        public async Task<ProductDTO> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            product.CategoryName = category.Name;
            return product;
        }

        public async Task<IEnumerable<ProductDTO>> GetListAsync(PaginationParams paginationParams)
        {

            var products = await _repository.GetListAsync(paginationParams );

            foreach(var product in products)
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                product.CategoryName = category.Name;
            }

            return products;
        }

        public async Task<ProductDTO> UpdateAsync(ProductDTO entity)
        {
            ValidateProduct(entity);
            var product = await _repository.GetByIdAsync(entity.Id);
            var category = await _categoryRepository.GetByIdAsync(entity.CategoryId);
            var result = await _repository.UpdateAsync(entity);

            return result;
        }
    }
}
