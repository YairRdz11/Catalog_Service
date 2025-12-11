using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CatalogService.API.Controllers.v1;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters;
using CatalogService.Transversal.Classes.Models;
using CatalogService.Transversal.Interfaces.BL;
using Common.Utilities.Classes.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CatalogService.Testing.UnitTesting.CatalogService.API.Testing
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _serviceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        private ProductsController CreateController()
        {
            return new ProductsController(_serviceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsOk_WithMappedModels()
        {
            // Arrange
            var controller = CreateController();
            var filter = new ProductFilterParams { PageNumber =1, PageSize =10 };
            var dtos = new List<ProductDTO> { new ProductDTO { Id = Guid.NewGuid(), Name = "P1" } };
            var models = new List<ProductModel> { new ProductModel { Id = dtos[0].Id, Name = "P1" } };

            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(dtos);
            _mapperMock.Setup(m => m.Map<List<ProductModel>>(dtos)).Returns(models);

            // Act
            var actionResult = await controller.GetAllProductsAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.Equal(200, response.Status);
            var payload = Assert.IsAssignableFrom<List<ProductModel>>(response.Result);
            Assert.Single(payload);
            Assert.Equal(models[0].Id, payload[0].Id);
        }

        [Fact]
        public async Task CreateProductAsync_ReturnsCreated_WithMappedModel()
        {
            // Arrange
            var controller = CreateController();
            var inputModel = new CreateProductModel { Name = "New", Price =10m, Amount =1, CategoryId = Guid.NewGuid() };
            var dtoInput = new ProductDTO { Name = "New", Price =10m, Amount =1, CategoryId = inputModel.CategoryId };
            var dtoCreated = new ProductDTO { Id = Guid.NewGuid(), Name = "New" };
            var modelCreated = new ProductModel { Id = dtoCreated.Id, Name = "New" };

            _mapperMock.Setup(m => m.Map<ProductDTO>(inputModel)).Returns(dtoInput);
            _serviceMock.Setup(s => s.CreateAsync(dtoInput)).ReturnsAsync(dtoCreated);
            _mapperMock.Setup(m => m.Map<ProductModel>(dtoCreated)).Returns(modelCreated);

            // Act
            var actionResult = await controller.CreateProductAsync(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.Equal(201, response.Status);
            var payload = Assert.IsType<ProductModel>(response.Result);
            Assert.Equal(modelCreated.Id, payload.Id);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsOk_WithMappedModel()
        {
            // Arrange
            var controller = CreateController();
            var id = Guid.NewGuid();
            var dto = new ProductDTO { Id = id, Name = "P" };
            var model = new ProductModel { Id = id, Name = "P" };

            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);
            _mapperMock.Setup(m => m.Map<ProductModel>(dto)).Returns(model);

            // Act
            var actionResult = await controller.GetProductByIdAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.Equal(200, response.Status);
            var payload = Assert.IsType<ProductModel>(response.Result);
            Assert.Equal(id, payload.Id);
        }

        [Fact]
        public async Task DeleteProductByIdAsync_ReturnsOk_WithModel()
        {
            // Arrange
            var controller = CreateController();
            var id = Guid.NewGuid();
            var dtoDeleted = new ProductDTO { Id = id, Name = "Del" };
            var modelDeleted = new ProductModel { Id = id, Name = "Del" };

            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(dtoDeleted);
            _mapperMock.Setup(m => m.Map<ProductModel>(dtoDeleted)).Returns(modelDeleted);

            // Act
            var actionResult = await controller.DeleteProductByIdAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.Equal(200, response.Status);
            var payload = Assert.IsType<ProductModel>(response.Result);
            Assert.Equal(id, payload.Id);
        }

        [Fact]
        public async Task UpdateProductAsync_ReturnsOk_WithUpdatedModel()
        {
            // Arrange
            var controller = CreateController();
            var id = Guid.NewGuid();
            var inputModel = new CreateProductModel { Name = "Upd", Price =11m, Amount =2, CategoryId = Guid.NewGuid() };
            var dtoInput = new ProductDTO { Id = id, Name = "Upd", Price =11m, Amount =2, CategoryId = inputModel.CategoryId };
            var dtoUpdated = new ProductDTO { Id = id, Name = "Upd" };
            var modelUpdated = new ProductModel { Id = id, Name = "Upd" };

            _mapperMock.Setup(m => m.Map<ProductDTO>(inputModel)).Returns(dtoInput);
            _serviceMock.Setup(s => s.UpdateAsync(It.Is<ProductDTO>(d => d.Id == id && d.Name == "Upd")))
            .ReturnsAsync(dtoUpdated);
            _mapperMock.Setup(m => m.Map<ProductModel>(dtoUpdated)).Returns(modelUpdated);

            // Act
            var actionResult = await controller.UpdateProductAsync(id, inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.Equal(200, response.Status);
            var payload = Assert.IsType<ProductModel>(response.Result);
            Assert.Equal(id, payload.Id);
        }
    }
}
