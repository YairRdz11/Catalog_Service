using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.DAL.Classes.Mapping;
using CatalogService.DAL.Classes.Repositories;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters; // added
using Common.Utilities.Classes.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Testing.CatalogService.DAL.Testing
{
    public class ProductRepositoryTests
    {
        private static ProductRepository CreateRepository(out CatalogBDContext context)
        {
            var options = new DbContextOptionsBuilder<CatalogBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            context = new CatalogBDContext(options);
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>());
            var mapper = mapperConfig.CreateMapper();
            return new ProductRepository(context, mapper);
        }

        private static ProductDTO BuildDto(string name, Guid categoryId, string? description = null, decimal price =0m, int amount =0, string? url = null)
        {
            return new ProductDTO
            {
                Id = Guid.Empty,
                Name = name,
                CategoryId = categoryId,
                Description = description,
                Price = price,
                Amount = amount,
                URL = url
            };
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistAndReturnProduct()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            var dto = BuildDto("Laptop", category.Id, "Desc",1200m,5, "https://image");

            var created = await repo.CreateAsync(dto);

            created.Should().NotBeNull();
            created.Id.Should().NotBe(Guid.Empty);
            created.Name.Should().Be("Laptop");
            created.CategoryId.Should().Be(category.Id);
            context.Products.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Cat" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Phone", CategoryId = category.Id, Price =10m, Amount =2 };
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(product.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(product.Id);
            result.Name.Should().Be("Phone");
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ShouldThrow()
        {
            var repo = CreateRepository(out _);
            Func<Task> act = () => repo.GetByIdAsync(Guid.NewGuid());
            await act.Should().ThrowAsync<NotFoundException>()
            .Where(e => e.Message.Contains("Product"));
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnOrderedPagedProducts()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Cat" };
            context.Categories.Add(category);
            context.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Zeta", CategoryId = category.Id },
            new Product { Id = Guid.NewGuid(), Name = "Alpha", CategoryId = category.Id },
            new Product { Id = Guid.NewGuid(), Name = "Middle", CategoryId = category.Id }
            );
            await context.SaveChangesAsync();

            var filter = new ProductFilterParams { PageNumber =1, PageSize =10 };
            var list = await repo.GetListAsync(filter);
            list.Should().HaveCount(3);
            list.Select(p => p.Name).Should().ContainInOrder("Alpha", "Middle", "Zeta");
        }

        [Fact]
        public async Task GetListAsync_WithCategoryFilter_ShouldReturnOnlyCategory()
        {
            var repo = CreateRepository(out var context);
            var cat1 = new Category { Id = Guid.NewGuid(), Name = "Cat1" };
            var cat2 = new Category { Id = Guid.NewGuid(), Name = "Cat2" };
            context.Categories.AddRange(cat1, cat2);
            context.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "A", CategoryId = cat1.Id },
            new Product { Id = Guid.NewGuid(), Name = "B", CategoryId = cat1.Id },
            new Product { Id = Guid.NewGuid(), Name = "C", CategoryId = cat2.Id }
            );
            await context.SaveChangesAsync();

            var filter = new ProductFilterParams { CategoryId = cat1.Id, PageNumber =1, PageSize =10 };
            var list = await repo.GetListAsync(filter);
            list.Should().HaveCount(2);
            list.All(p => p.CategoryId == cat1.Id).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyFields()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Cat" };
            context.Categories.Add(category);
            var product = new Product { Id = Guid.NewGuid(), Name = "Old", CategoryId = category.Id, Description = "OldDesc", ImageUrl = "http://old", Price =5m, Amount =1 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var updateDto = new ProductDTO
            {
                Id = product.Id,
                Name = "NewName",
                CategoryId = category.Id,
                Description = "NewDesc",
                URL = "https://new",
                Price =10m,
                Amount =3
            };

            var updated = await repo.UpdateAsync(updateDto);
            updated.Name.Should().Be("NewName");
            updated.Description.Should().Be("NewDesc");
            updated.URL.Should().Be("https://new");
            updated.Price.Should().Be(10m);
            updated.Amount.Should().Be(3);
            var tracked = await context.Products.FirstAsync();
            tracked.ImageUrl.Should().Be("https://new");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveAndReturnProduct()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Cat" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Temp", CategoryId = category.Id };
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var deleted = await repo.DeleteAsync(product.Id);
            deleted.Id.Should().Be(product.Id);
            context.Products.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ShouldThrow()
        {
            var repo = CreateRepository(out _);
            Func<Task> act = () => repo.DeleteAsync(Guid.NewGuid());
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DoesItemExistByNameAsync_ShouldReturnTrueForExistingIgnoringCaseAndWhitespace()
        {
            var repo = CreateRepository(out var context);
            var category = new Category { Id = Guid.NewGuid(), Name = "Cat" };
            context.Categories.Add(category);
            context.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Gaming", CategoryId = category.Id });
            await context.SaveChangesAsync();

            var exists = await repo.DoesItemExistByNameAsync(" gaming ");
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task DoesItemExistByNameAsync_ShouldReturnFalseForNonExisting()
        {
            var repo = CreateRepository(out _);
            var exists = await repo.DoesItemExistByNameAsync("NonExisting");
            exists.Should().BeFalse();
        }
    }
}
