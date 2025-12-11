using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CatalogService.API.Controllers.v1;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Models;
using CatalogService.Transversal.Interfaces.BL;
using Common.Utilities.Classes.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CatalogService.Testing.UnitTesting.CatalogService.API.Testing
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        private CategoriesController CreateController()
        {
            return new CategoriesController(_serviceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsOk_WithMappedModels()
        {
            // Arrange
            var controller = CreateController();
            var dtos = new List<CategoryDTO> { new CategoryDTO { Id = Guid.NewGuid(), Name = "Cat1" } };
            var models = new List<CategoryModel> { new CategoryModel { CategoryId = dtos[0].Id, Name = "Cat1" } };

            _serviceMock.Setup(s => s.GetListAsync()).ReturnsAsync(dtos);
            _mapperMock.Setup(m => m.Map<List<CategoryModel>>(dtos)).Returns(models);

            // Act
            var actionResult = await controller.GetCategoriesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
             var response = Assert.IsType<ApiResponse>(okResult.Value);
             Assert.Equal(200, response.Status);
             var payload = Assert.IsAssignableFrom<List<CategoryModel>>(response.Result);
             Assert.Single(payload);
             Assert.Equal(models[0].CategoryId, payload[0].CategoryId);
         }

         [Fact]
         public async Task CreateCategoryAsync_ReturnsCreated_WithMappedModel()
         {
             // Arrange
             var controller = CreateController();
             var inputModel = new CreateCategoryModel { Name = "New" };
             var dtoInput = new CategoryDTO { Name = "New" };
             var dtoCreated = new CategoryDTO { Id = Guid.NewGuid(), Name = "New" };
             var modelCreated = new CategoryModel { CategoryId = dtoCreated.Id, Name = "New" };

             _mapperMock.Setup(m => m.Map<CategoryDTO>(inputModel)).Returns(dtoInput);
             _serviceMock.Setup(s => s.CreateAsync(dtoInput)).ReturnsAsync(dtoCreated);
             _mapperMock.Setup(m => m.Map<CategoryModel>(dtoCreated)).Returns(modelCreated);

             // Act
             var actionResult = await controller.CreateCategoryAsync(inputModel);

             // Assert
             var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
             var response = Assert.IsType<ApiResponse>(okResult.Value);
             Assert.Equal(201, response.Status);
             var payload = Assert.IsType<CategoryModel>(response.Result);
             Assert.Equal(modelCreated.CategoryId, payload.CategoryId);
         }

         [Fact]
         public async Task GetCategoryByIdAsync_ReturnsOk_WithMappedModel()
         {
             // Arrange
             var controller = CreateController();
             var id = Guid.NewGuid();
             var dto = new CategoryDTO { Id = id, Name = "Cat" };
             var model = new CategoryModel { CategoryId = id, Name = "Cat" };

             _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);
             _mapperMock.Setup(m => m.Map<CategoryModel>(dto)).Returns(model);

             // Act
             var actionResult = await controller.GetCategoryByIdAsync(id);

             // Assert
             var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
             var response = Assert.IsType<ApiResponse>(okResult.Value);
             Assert.Equal(200, response.Status);
             var payload = Assert.IsType<CategoryModel>(response.Result);
             Assert.Equal(id, payload.CategoryId);
         }

         [Fact]
         public async Task DeleteCategoryAsync_ReturnsOk_WithDeletedModel()
         {
             // Arrange
             var controller = CreateController();
             var id = Guid.NewGuid();
             var dtoDeleted = new CategoryDTO { Id = id, Name = "Del" };
             var modelDeleted = new CategoryModel { CategoryId = id, Name = "Del" };

             _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(dtoDeleted);
             _mapperMock.Setup(m => m.Map<CategoryModel>(dtoDeleted)).Returns(modelDeleted);

             // Act
             var actionResult = await controller.DeleteCategoryAsync(id);

             // Assert
             var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
             var response = Assert.IsType<ApiResponse>(okResult.Value);
             Assert.Equal(200, response.Status);
             var payload = Assert.IsType<CategoryModel>(response.Result);
             Assert.Equal(id, payload.CategoryId);
         }

         [Fact]
         public async Task UpdateCategoryAsync_ReturnsOk_WithUpdatedModel()
         {
             // Arrange
             var controller = CreateController();
             var id = Guid.NewGuid();
             var inputModel = new CreateCategoryModel { Name = "Upd" };
             var dtoInput = new CategoryDTO { Id = id, Name = "Upd" };
             var dtoUpdated = new CategoryDTO { Id = id, Name = "Upd" };
             var modelUpdated = new CategoryModel { CategoryId = id, Name = "Upd" };

             _mapperMock.Setup(m => m.Map<CategoryDTO>(inputModel)).Returns(dtoInput);
             _serviceMock.Setup(s => s.UpdateAsync(It.Is<CategoryDTO>(d => d.Id == id && d.Name == "Upd")))
             .ReturnsAsync(dtoUpdated);
             _mapperMock.Setup(m => m.Map<CategoryModel>(dtoUpdated)).Returns(modelUpdated);

             // Act
             var actionResult = await controller.UpdateCategoryAsync(id, inputModel);

             // Assert
             var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
             var response = Assert.IsType<ApiResponse>(okResult.Value);
             Assert.Equal(200, response.Status);
             var payload = Assert.IsType<CategoryModel>(response.Result);
             Assert.Equal(id, payload.CategoryId);
         }
    }
}
