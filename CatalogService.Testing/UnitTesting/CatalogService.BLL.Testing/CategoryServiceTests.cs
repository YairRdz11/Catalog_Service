using CatalogService.BLL.Classes;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Interfaces.DAL;
using Common.Utilities.Classes.Exceptions;
using FluentAssertions;
using Moq;

namespace CatalogService.Testing.UnitTesting.CatalogService.BLL.Testing
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object, _mockProductRepository.Object);
        }

#region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidCategory_ShouldReturnCreatedCategory()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "Electronics",
                Description = "Electronic items",
                URL = "https://example.com/electronics",
                ParentCategoryId = Guid.Empty
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.CreateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync((CategoryDTO dto) => dto);

            // Act
            var result = await _categoryService.CreateAsync(categoryDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Electronics");
            result.Id.Should().NotBe(Guid.Empty);
            _mockCategoryRepository.Verify(r => r.CreateAsync(It.IsAny<CategoryDTO>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithParentCategory_ShouldVerifyParentExists()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var parentCategory = new CategoryDTO
            {
                Id = parentId,
                Name = "Parent Category",
                Description = "Parent",
                URL = "https://example.com/parent"
            };

            var categoryDTO = new CategoryDTO
            {
                Name = "Child Category",
                Description = "Child",
                URL = "https://example.com/child",
                ParentCategoryId = parentId
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(parentId))
                .ReturnsAsync(parentCategory);

            _mockCategoryRepository
                .Setup(r => r.CreateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync((CategoryDTO dto) => dto);

            // Act
            var result = await _categoryService.CreateAsync(categoryDTO);

            // Assert
            result.Should().NotBeNull();
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockCategoryRepository.Verify(r => r.CreateAsync(It.IsAny<CategoryDTO>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithEmptyName_ShouldThrowValidateException()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "",
                Description = "Test",
                URL = "https://example.com/test"
            };

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public async Task CreateAsync_WithNameTooLong_ShouldThrowValidateException()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = new string('A', 51), // Exceeds 50 characters
                Description = "Test",
                URL = "https://example.com/test"
            };

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("50"));
        }

        [Fact]
        public async Task CreateAsync_WithInvalidURL_ShouldThrowValidateException()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "ValidName",
                Description = "Test",
                URL = "not-a-valid-url"
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "URL");
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ShouldThrowValidateException()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "Electronics",
                Description = "Test",
                URL = "https://example.com/test"
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            await act.Should()
                .ThrowAsync<ValidateException>()
                .Where(ex => ex.Errors.Any(e => e.ErrorMessage.Contains("already exists")));
        }

        [Fact]
        public async Task CreateAsync_WithDescriptionTooLong_ShouldThrowValidateException()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "ValidName",
                Description = new string('A', 501), // Exceeds 500 characters
                URL = "https://example.com/test"
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("500"));
        }

#endregion

#region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategoryWithProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics",
                Description = "Electronic items"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ThrowsAsync(new NotFoundException("Category", categoryId));

            // Act
            Func<Task> act = async () => await _categoryService.GetByIdAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

#endregion

#region GetListAsync Tests

        [Fact]
        public async Task GetListAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO { Id = Guid.NewGuid(), Name = "Electronics" },
                new CategoryDTO { Id = Guid.NewGuid(), Name = "Books" },
                new CategoryDTO { Id = Guid.NewGuid(), Name = "Clothing" }
            };

            _mockCategoryRepository
                .Setup(r => r.GetListAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Name == "Electronics");
            result.Should().Contain(c => c.Name == "Books");
            result.Should().Contain(c => c.Name == "Clothing");
        }

        [Fact]
        public async Task GetListAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockCategoryRepository
                .Setup(r => r.GetListAsync())
                .ReturnsAsync(new List<CategoryDTO>());

            // Act
            var result = await _categoryService.GetListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

#endregion

#region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidCategory_ShouldReturnUpdatedCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics",
                Description = "Old description"
            };

            var updatedCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Updated Electronics",
                Description = "New description",
                URL = "https://example.com/updated"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(updatedCategory.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.UpdateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _categoryService.UpdateAsync(updatedCategory);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("Updated Electronics");
            result.Description.Should().Be("New description");
            _mockCategoryRepository.Verify(r => r.UpdateAsync(It.IsAny<CategoryDTO>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithParentCategory_ShouldVerifyParentExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            var existingCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Child Category",
                ParentCategoryId = Guid.Empty
            };

            var parentCategory = new CategoryDTO
            {
                Id = parentId,
                Name = "Parent Category"
            };

            var updatedCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Updated Child Category",
                Description = "Updated",
                URL = "https://example.com/child",
                ParentCategoryId = parentId
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(parentId))
                .ReturnsAsync(parentCategory);

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(updatedCategory.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.UpdateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _categoryService.UpdateAsync(updatedCategory);

            // Assert
            result.Should().NotBeNull();
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockCategoryRepository.Verify(r => r.UpdateAsync(It.IsAny<CategoryDTO>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidData_ShouldThrowValidateException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics"
            };

            var updatedCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "", // Invalid: empty name
                Description = "Test"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockCategoryRepository
                .Setup(r => r.UpdateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync(updatedCategory);
            // Act
            Func<Task> act = async () => await _categoryService.UpdateAsync(updatedCategory);

            // Assert
            await act.Should().ThrowAsync<ValidateException>();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updatedCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Non-existent",
                Description = "Test"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ThrowsAsync(new NotFoundException("Category", categoryId));

            // Act
            Func<Task> act = async () => await _categoryService.UpdateAsync(updatedCategory);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

#endregion

#region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldReturnDeletedCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var deletedCategory = new CategoryDTO
            {
                Id = categoryId,
                Name = "Electronics",
                Description = "Electronic items"
            };

            _mockCategoryRepository
                .Setup(r => r.DeleteAsync(categoryId))
                .ReturnsAsync(deletedCategory);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("Electronics");
            _mockCategoryRepository.Verify(r => r.DeleteAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockCategoryRepository.
                Setup(r => r.DeleteAsync(categoryId))
                .ThrowsAsync(new NotFoundException("Category", categoryId));

            // Act
            Func<Task> act = async () => await _categoryService.DeleteAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

#endregion

#region Edge Cases and Integration Tests

        [Fact]
        public async Task CreateAsync_WithNullURL_ShouldSucceed()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "Electronics",
                Description = "Electronic items",
                URL = null
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.CreateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync((CategoryDTO dto) => dto);

            // Act
            var result = await _categoryService.CreateAsync(categoryDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_WithNullDescription_ShouldSucceed()
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "Electronics",
                Description = null,
                URL = "https://example.com/test"
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(categoryDTO.Name))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.CreateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync((CategoryDTO dto) => dto);

            // Act
            var result = await _categoryService.CreateAsync(categoryDTO);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByIdAsync_CategoryWithNoProducts_ShouldReturnEmptyProductList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryDTO = new CategoryDTO
            {
                Id = categoryId,
                Name = "Empty Category"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(categoryDTO);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://example.com")]
        [InlineData("https://example.com/path/to/resource")]
        [InlineData("http://subdomain.example.com:8080")]
        public async Task CreateAsync_WithValidURLFormats_ShouldSucceed(string url)
        {
            // Arrange
            var categoryDTO = new CategoryDTO
            {
                Name = "TestCategory",
                Description = "Test",
                URL = url
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(r => r.CreateAsync(It.IsAny<CategoryDTO>()))
                .ReturnsAsync((CategoryDTO dto) => dto);

            // Act
            var result = await _categoryService.CreateAsync(categoryDTO);

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
            var categoryDTO = new CategoryDTO
            {
                Name = "TestCategory",
                Description = "Test",
                URL = url
            };

            _mockCategoryRepository
                .Setup(r => r.DoesItemExistByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(categoryDTO);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidateException>();
            exception.Which.Errors.Should().Contain(e => e.PropertyName == "URL");
        }

#endregion
  }
}
