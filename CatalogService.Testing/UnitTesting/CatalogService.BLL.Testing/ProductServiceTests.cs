using CatalogService.BLL.Classes;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters; // added for ProductFilterParams
using CatalogService.Transversal.Interfaces.DAL;
using Common.Utilities.Classes.Exceptions;
using FluentAssertions;
using Moq;

namespace CatalogService.Testing.UnitTesting.CatalogService.BLL.Testing
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _productService = new ProductService(_mockProductRepository.Object, _mockCategoryRepository.Object);
        }

#region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidProduct_ShouldReturnCreatedProduct()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Amount = 10,
                URL = "https://example.com/laptop",
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Laptop");
            result.Price.Should().Be(999.99m);
            result.Amount.Should().Be(10);
            result.Id.Should().NotBe(Guid.Empty);
            _mockProductRepository.Verify(r => r.CreateAsync(It.IsAny<ProductDTO>()), Times.Once);
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentCategory_ShouldThrowNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ThrowsAsync(new NotFoundException("Category", categoryId));

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_WithEmptyName_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public async Task CreateAsync_WithNameTooLong_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = new string('A', 101), // Exceeds 100 characters
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
        }

        [Fact]
        public async Task CreateAsync_WithNegativePrice_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "Test",
                Price = -10m,
                Amount = 5,
                CategoryId = categoryId
            };

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Price");
        }

        [Fact]
        public async Task CreateAsync_WithNegativeAmount_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "Test",
                Price = 100m,
                Amount = -5,
                CategoryId = categoryId
            };

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Amount");
        }

        [Fact]
        public async Task CreateAsync_WithDescriptionTooLong_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = new string('A', 1001), // Exceeds 1000 characters
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            _mockProductRepository.
                Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("1000"));
        }

        [Fact]
        public async Task CreateAsync_WithInvalidURL_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                URL = "not-a-valid-url",
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "URL");
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            await act.Should().ThrowAsync<ValidateException>()
                .Where(ex => ex.Errors.Any(e => e.ErrorMessage.Contains("already exists")));
        }

        [Fact]
        public async Task CreateAsync_WithZeroPrice_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Free Item",
                Description = "Free promotional item",
                Price = 0m,
                Amount = 10,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Price.Should().Be(0m);
        }

        [Fact]
        public async Task CreateAsync_WithZeroAmount_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Out of Stock",
                Description = "Currently unavailable",
                Price = 100m,
                Amount = 0,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(0);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnProductWithCategoryName()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var productDTO = new ProductDTO
            {
                Id = productId,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = categoryId
            };

            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(productDTO);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Laptop");
            result.CategoryName.Should().Be("Electronics");
            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ThrowsAsync(new NotFoundException("Product", productId));

            // Act
            Func<Task> act = async () => await _productService.GetByIdAsync(productId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        #endregion

        #region GetListAsync Tests

        [Fact]
        public async Task GetListAsync_ShouldReturnAllProductsWithCategoryNames()
        {
            // Arrange
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();

            var products = new List<ProductDTO>
            {
                new ProductDTO { Id = Guid.NewGuid(), Name = "Laptop", CategoryId = category1Id },
                new ProductDTO { Id = Guid.NewGuid(), Name = "Mouse", CategoryId = category1Id },
                new ProductDTO { Id = Guid.NewGuid(), Name = "Book", CategoryId = category2Id }
            };

            var category1 = new CategoryDTO { Id = category1Id, Name = "Electronics" };
            var category2 = new CategoryDTO { Id = category2Id, Name = "Books" };

            _mockProductRepository
                .Setup(r => r.GetListAsync(It.IsAny<ProductFilterParams>()))
                .ReturnsAsync(products);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(category1Id))
                .ReturnsAsync(category1);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(category2Id))
                .ReturnsAsync(category2);

            var filter = new ProductFilterParams { PageNumber =1, PageSize =20 };

            // Act
            var result = await _productService.GetListAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.Name == "Laptop" && p.CategoryName == "Electronics");
            result.Should().Contain(p => p.Name == "Mouse" && p.CategoryName == "Electronics");
            result.Should().Contain(p => p.Name == "Book" && p.CategoryName == "Books");
        }

        [Fact]
        public async Task GetListAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockProductRepository
                .Setup(r => r.GetListAsync(It.IsAny<ProductFilterParams>()))
                .ReturnsAsync(new List<ProductDTO>());

            var filter = new ProductFilterParams { PageNumber =1, PageSize =10 };

            // Act
            var result = await _productService.GetListAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetListAsync_WithMultipleCalls_ShouldCallCategoryRepositoryForEachProduct()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var products = new List<ProductDTO>
            {
                new ProductDTO { Id = Guid.NewGuid(), Name = "Product1", CategoryId = categoryId },
                new ProductDTO { Id = Guid.NewGuid(), Name = "Product2", CategoryId = categoryId },
                new ProductDTO { Id = Guid.NewGuid(), Name = "Product3", CategoryId = categoryId }
            };

            var category = new CategoryDTO { Id = categoryId, Name = "Electronics" };

            _mockProductRepository
                .Setup(r => r.GetListAsync(It.IsAny<ProductFilterParams>()))
                .ReturnsAsync(products);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var filter = new ProductFilterParams { PageNumber =1, PageSize =50 };

            // Act
            var result = await _productService.GetListAsync(filter);

            // Assert
            result.Should().HaveCount(3);
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId), Times.Exactly(3));
        }

    #endregion

    #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidProduct_ShouldReturnUpdatedProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var existingProduct = new ProductDTO
            {
                Id = productId,
                Name = "Old Laptop",
                Description = "Old description",
                Price = 500m,
                Amount = 5,
                CategoryId = categoryId
            };

            var updatedProduct = new ProductDTO
            {
                Id = productId,
                Name = "Updated Laptop",
                Description = "New description",
                Price = 999.99m,
                Amount = 10,
                URL = "https://example.com/laptop",
                CategoryId = categoryId
            };

            var category = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockProductRepository
                .Setup(r => r.UpdateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _productService.UpdateAsync(updatedProduct);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Updated Laptop");
            result.Price.Should().Be(999.99m);
            _mockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductDTO>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidData_ShouldThrowValidateException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var existingProduct = new ProductDTO
            {
                Id = productId,
                Name = "Laptop",
                CategoryId = categoryId
            };

            var updatedProduct = new ProductDTO
            {
                Id = productId,
                Name = "", // Invalid: empty name
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockProductRepository
                .Setup(r => r.UpdateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync(updatedProduct);

            // Act
            Func<Task> act = async () => await _productService.UpdateAsync(updatedProduct);

            // Assert
            await act.Should().ThrowAsync<ValidateException>();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
        {
                // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var updatedProduct = new ProductDTO
            {
                Id = productId,
                Name = "Non-existent",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ThrowsAsync(new NotFoundException("Product", productId));

            // Act
            Func<Task> act = async () => await _productService.UpdateAsync(updatedProduct);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentCategory_ShouldThrowNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var existingProduct = new ProductDTO
            {
                Id = productId,
                Name = "Laptop",
                CategoryId = categoryId
            };

            var updatedProduct = new ProductDTO
            {
                Id = productId,
                Name = "Updated Laptop",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ThrowsAsync(new NotFoundException("Category", categoryId));

            // Act
            Func<Task> act = async () => await _productService.UpdateAsync(updatedProduct);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldReturnDeletedProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var deletedProduct = new ProductDTO
            {
                Id = productId,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Amount = 10
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(deletedProduct);

            _mockProductRepository
                .Setup(r => r.DeleteAsync(productId))
                .ReturnsAsync(deletedProduct);

            // Act
            var result = await _productService.DeleteAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Laptop");
            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ThrowsAsync(new NotFoundException("Product", productId));

            // Act
            Func<Task> act = async () => await _productService.DeleteAsync(productId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

    #endregion

    #region Edge Cases and Integration Tests

        [Fact]
        public async Task CreateAsync_WithNullDescription_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = null,
                Price = 999.99m,
                Amount = 10,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task CreateAsync_WithNullURL_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Laptop",
                Description = "Test",
                Price = 999.99m,
                Amount = 10,
                URL = null,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Laptop");
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://example.com")]
        [InlineData("https://example.com/path/to/product")]
        [InlineData("http://subdomain.example.com:8080/product")]
        public async Task CreateAsync_WithValidURLFormats_ShouldSucceed(string url)
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "TestProduct",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                URL = url,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.URL.Should().Be(url);
        }

        [Theory]
        [InlineData("ftp://example.com")]
        [InlineData("file:///C:/path")]
        [InlineData("javascript:alert('test')")]
        public async Task CreateAsync_WithInvalidURLSchemes_ShouldThrowValidateException(string url)
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productDTO = new ProductDTO
            {
                Name = "TestProduct",
                Description = "Test",
                Price = 100m,
                Amount = 5,
                URL = url,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _productService.CreateAsync(productDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "URL");
        }

        [Fact]
        public async Task CreateAsync_WithMaxValidPrice_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Expensive Item",
                Description = "Very expensive",
                Price = 999999999.99m,
                Amount = 1,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Price.Should().Be(999999999.99m);
        }

        [Fact]
        public async Task CreateAsync_WithMaxValidAmount_ShouldSucceed()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var productDTO = new ProductDTO
            {
                Name = "Large Stock Item",
                Description = "Large inventory",
                Price = 100m,
                Amount = int.MaxValue,
                CategoryId = categoryId
            };

            _mockProductRepository
                .Setup(r => r.DoesItemExistByNameAsync(productDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            _mockProductRepository
                .Setup(r => r.CreateAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync((ProductDTO dto) => dto);

            // Act
            var result = await _productService.CreateAsync(productDTO);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(int.MaxValue);
        }

    #endregion
    }
}
