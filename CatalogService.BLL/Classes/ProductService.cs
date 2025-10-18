using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Exceptions;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using Utilities.Classes.Common;

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
            productDTO.ProductId = Guid.NewGuid();
            ValidateProduct(productDTO);
            var category = await _categoryRepository.GetByIdAsync(productDTO.CategoryId);
            var result = await _repository.CreateAsync(productDTO);
            return result;
        }

        private void ValidateProduct(ProductDTO productDTO)
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
        }

        public Task<ProductDTO> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductDTO> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            product.CategoryName = category.Name;
            return product;
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
