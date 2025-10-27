using AutoMapper;
using CatalogService.DAL.Classes.Data;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.DAL.Classes.Mapping;
using CatalogService.DAL.Classes.Repositories;
using CatalogService.Transversal.Classes.Dtos;
using Common.Utilities.Classes.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Testing.CatalogService.DAL.Testing
{
    public class CategoryRepositoryTests
    {
        private static CategoryRepository CreateRepository(out CatalogBDContext context)
        {
            var options = new DbContextOptionsBuilder<CatalogBDContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new CatalogBDContext(options);

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>());
            var mapper = mapperConfig.CreateMapper();
            return new CategoryRepository(context, mapper);
        }

        private static CategoryDTO BuildDto(string name, string? description = null, string? url = null, Guid? parentId = null)
        {
            return new CategoryDTO
            {
                 Id = Guid.Empty, // let repository / EF create id
                 Name = name,
                 Description = description ?? string.Empty,
                 URL = url ?? string.Empty,
                 ParentCategoryId = parentId ?? Guid.Empty
            };
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistAndReturnCategory()
        {
            var repo = CreateRepository(out var context);
            var dto = BuildDto("Electronics", "Desc", "https://example.com");

            var created = await repo.CreateAsync(dto);

            created.Should().NotBeNull();
            created.Id.Should().NotBe(Guid.Empty);
            created.Name.Should().Be("Electronics");
            context.Categories.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory()
        {
            var repo = CreateRepository(out var context);
            var entity = new Category { Id = Guid.NewGuid(), Name = "Books" };
            context.Categories.Add(entity);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(entity.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.Name.Should().Be("Books");
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ShouldThrow()
        {
            var repo = CreateRepository(out _);
            var id = Guid.NewGuid();
            Func<Task> act = () => repo.GetByIdAsync(id);
            await act.Should()
                    .ThrowAsync<NotFoundException>()
                    .Where(e => 
                                e.Message.Contains("Category") && 
                                e.Message.Contains(id.ToString()));
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnOrderedList()
        {
            var repo = CreateRepository(out var context);
            context.Categories.AddRange(
                new Category { Id = Guid.NewGuid(), Name = "Zeta" },
                new Category { Id = Guid.NewGuid(), Name = "Alpha" },
                new Category { Id = Guid.NewGuid(), Name = "Middle" }
            );
            await context.SaveChangesAsync();

            var list = await repo.GetListAsync();
            list.Should().HaveCount(3);
            list.Select(c => c.Name).Should().ContainInOrder("Alpha", "Middle", "Zeta");
        }

        [Fact]
        public async Task UpdateAsync_ShouldChangeFields()
        {
            var repo = CreateRepository(out var context);
            var existing = new Category { Id = Guid.NewGuid(), Name = "Old", Description = "OldDesc", URL = "http://old", ParentCategoryId = null };
            context.Categories.Add(existing);
            await context.SaveChangesAsync();

            var updateDto = new CategoryDTO
            {
                Id = existing.Id,
                Name = "NewName",
                Description = "NewDesc",
                URL = "https://new",
                ParentCategoryId = Guid.Empty // should null out
            };

            var updated = await repo.UpdateAsync(updateDto);
            updated.Name.Should().Be("NewName");
            updated.Description.Should().Be("NewDesc");
            updated.URL.Should().Be("https://new");
            updated.ParentCategoryId.Should().Be(Guid.Empty); // mapped from null
            var tracked = await context.Categories.FirstAsync();
            tracked.ParentCategoryId.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldAssignParentCategory()
        {
            var repo = CreateRepository(out var context);
            var parent = new Category { Id = Guid.NewGuid(), Name = "Parent" };
            var child = new Category { Id = Guid.NewGuid(), Name = "Child" };
            context.Categories.AddRange(parent, child);
            await context.SaveChangesAsync();

            var updateDto = new CategoryDTO
            {
                Id = child.Id,
                Name = "ChildUpdated",
                Description = "Desc",
                URL = "https://child",
                ParentCategoryId = parent.Id
            };

            var updated = await repo.UpdateAsync(updateDto);
            updated.ParentCategoryId.Should().Be(parent.Id);
            var trackedChild = await context.Categories.SingleAsync(c => c.Id == child.Id);
            trackedChild.ParentCategoryId.Should().Be(parent.Id);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveAndReturnEntity()
        {
            var repo = CreateRepository(out var context);
            var entity = new Category { Id = Guid.NewGuid(), Name = "Temp" };
            context.Categories.Add(entity);
            await context.SaveChangesAsync();

            var deleted = await repo.DeleteAsync(entity.Id);
            deleted.Id.Should().Be(entity.Id);
            context.Categories.Should().BeEmpty();
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
            context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Gaming" });
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
