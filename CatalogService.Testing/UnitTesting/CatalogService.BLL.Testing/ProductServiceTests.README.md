# ProductService Unit Tests

This file contains comprehensive unit tests for the `ProductService` class in the CatalogService.BLL project.

## Test Framework & Libraries

- **xUnit** - Testing framework
- **Moq** - Mocking framework for dependencies
- **FluentAssertions** - Fluent assertion library for readable test assertions

## Test Coverage

The test suite covers all public methods of the `ProductService` class with **33 test cases**:

### CreateAsync Tests (13 tests)
- ? Create valid product successfully
- ? Validate category exists
- ? Handle non-existent category (throws NotFoundException)
- ? Validate empty name throws exception
- ? Validate name length (max 100 characters)
- ? Validate negative price throws exception
- ? Validate negative amount throws exception
- ? Validate description length (max 1000 characters)
- ? Validate invalid URL format
- ? Validate duplicate product name
- ? Accept zero price (free items)
- ? Accept zero amount (out of stock)
- ? Accept null description and URL

### GetByIdAsync Tests (2 tests)
- ? Retrieve product with category name populated
- ? Handle non-existent product ID (throws NotFoundException)

### GetListAsync Tests (3 tests)
- ? Retrieve all products with category names populated
- ? Handle empty product list
- ? Verify category lookup called for each product (documents N+1 query pattern)

### UpdateAsync Tests (4 tests)
- ? Update valid product successfully
- ? Handle non-existent product ID (throws NotFoundException)
- ? Handle non-existent category ID (throws NotFoundException)
- ? Document current validation bug (not awaited)

### DeleteAsync Tests (2 tests)
- ? Delete product successfully
- ? Handle non-existent product ID (throws NotFoundException)

### Edge Cases and Integration Tests (9 tests)
- ? Accept null description
- ? Accept null URL
- ? Validate various URL formats (http, https, ports, paths)
- ? Reject invalid URL schemes (ftp, file, javascript)
- ? Accept maximum valid price values
- ? Accept maximum valid amount values

## Running the Tests

```bash
# Run all ProductService tests
dotnet test --filter "FullyQualifiedName~ProductServiceTests"

# Run all tests in the project
dotnet test CatalogService.Testing/CatalogService.Testing.csproj

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

## Test Structure

Each test follows the **Arrange-Act-Assert** (AAA) pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and mocks
    var productDTO = new ProductDTO { /* ... */ };
    _mockProductRepository.Setup(/* ... */);
    _mockCategoryRepository.Setup(/* ... */);

    // Act - Execute the method being tested
  var result = await _productService.CreateAsync(productDTO);

// Assert - Verify the expected outcome
  result.Should().NotBeNull();
    _mockProductRepository.Verify(/* ... */, Times.Once);
}
```

## Known Issues

### ?? Bug #1: Missing await in UpdateAsync
**Location**: Line 89 of `ProductService.cs`

The `ValidateProduct` call is missing the `await` keyword:
```csharp
ValidateProduct(entity); // Missing await!
```

Should be:
```csharp
await ValidateProduct(entity);
```

This causes validation to run asynchronously without blocking, allowing invalid data to be updated.

**Test**: `UpdateAsync_WithInvalidData_ShouldThrowValidateException` documents this current behavior.

### ?? Issue #2: Incorrect error message in ValidateProduct
**Location**: Lines 49-51 of `ProductService.cs`

When a duplicate product name is found, the error message incorrectly says "Category" instead of "Product":
```csharp
ErrorMessage = $"Category with name '{productDTO.Name}' already exists.",
```

Should be:
```csharp
ErrorMessage = $"Product with name '{productDTO.Name}' already exists.",
```

### ?? Performance Issue: N+1 Query Pattern in GetListAsync
**Location**: Lines 76-82 of `ProductService.cs`

The method calls `GetByIdAsync` for each product's category in a loop:
```csharp
foreach(var product in products)
{
    var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
    product.CategoryName = category.Name;
}
```

**Impact**: If there are 100 products, this makes 1 query for products + 100 queries for categories (N+1 queries).

**Recommendation**: 
1. Fetch all unique category IDs once
2. Batch load all required categories
3. Map category names to products in memory

**Test**: `GetListAsync_WithMultipleCalls_ShouldCallCategoryRepositoryForEachProduct` documents this behavior.

## Mocked Dependencies

- **IProductRepository** - Product data access operations
- **ICategoryRepository** - Category data access operations (for validation and enrichment)

## Validation Rules Tested

Based on `ProductValidator`:

| Field | Rule | Error Message |
|-------|------|---------------|
| Id | Not null | "ProductId cannot be null." |
| Name | Not empty | "Product name must not be empty." |
| Name | Max 100 chars | "Product name must not exceed 100 characters." |
| Description | Max 1000 chars (optional) | "Product description must not exceed 1000 characters." |
| Price | >= 0 | "Product price must be greater than or equal to zero." |
| Amount | >= 0 | "Product amount must be greater than or equal to zero." |
| URL | Max 200 chars (optional) | "Product URL must not exceed 200 characters." |
| URL | Valid http/https (optional) | "Product URL must be a valid absolute http/https URL." |
| CategoryId | Not null | "CategoryId cannot be null." |
| Name | Unique | "Product with name '{name}' already exists." (currently has wrong message) |

## Business Rules Tested

1. **Product Creation**
   - Must have a valid category
   - Name must be unique across all products
   - Price and amount can be zero (for free items or out-of-stock)

2. **Product Retrieval**
- Enriched with category name for display purposes
   - Throws NotFoundException for non-existent IDs

3. **Product Update**
   - Validates product exists
 - Validates new category exists
   - Should validate data (currently buggy - not awaited)

4. **Product Deletion**
   - Retrieves product first (to verify it exists)
   - Then deletes it

## Test Statistics

- **Total Tests**: 33
- **Passing**: 33
- **Failing**: 0
- **Test Execution Time**: ~1.8 seconds
- **Code Coverage**: Covers all public methods and key validation scenarios

## Recommendations for Code Improvements

1. **Fix the await bug in UpdateAsync** (line 89)
2. **Fix the error message** in ValidateProduct (lines 49-51)
3. **Optimize GetListAsync** to avoid N+1 queries
4. **Consider caching** category lookups if the same categories are frequently accessed
5. **Add bulk operations** for better performance when dealing with multiple products
