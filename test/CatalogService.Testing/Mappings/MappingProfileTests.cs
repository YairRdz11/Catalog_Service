using System.Collections.Generic;
using System.Linq;
using System;
using AutoMapper;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Models;
using CatalogService.Transversal.Mappings;
using Xunit;

namespace CatalogService.Testing.Mappings
{
     public class MappingProfileTests
     {
         private readonly IMapper _mapper;

         public MappingProfileTests()
         {
             var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
             config.AssertConfigurationIsValid();
             _mapper = config.CreateMapper();
         }

         [Fact]
         public void AutoMapper_Configuration_IsValid()
         {
             var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
             config.AssertConfigurationIsValid();
         }

         [Fact]
         public void CategoryDTO_Maps_To_CategoryModel()
         {
             var dto = new CategoryDTO
             {
                 Id = Guid.NewGuid(),
                 Name = "Cat Name",
                 Description = "Cat Desc",
                 URL = "https://cat",
                 ParentCategoryId = Guid.NewGuid(),
                 ParentCategoryName = "Parent Cat",
                 Products = new List<ProductDTO>
                 {
                    new ProductDTO { Id = Guid.NewGuid(), Name = "P1", Description = "D1", URL = "u1", CategoryName = "Cat Name", Price =9.99m, Amount =5, CategoryId = Guid.NewGuid() },
                    new ProductDTO { Id = Guid.NewGuid(), Name = "P2", Description = "D2", URL = "u2", CategoryName = "Cat Name", Price =19.99m, Amount =2, CategoryId = Guid.NewGuid() }
                 }
             };

             var model = _mapper.Map<CategoryModel>(dto);

             Assert.Equal(dto.Id, model.CategoryId);
             Assert.Equal(dto.Name, model.Name);
             Assert.Equal(dto.Description, model.Description);
             Assert.Equal(dto.URL, model.URL);
             Assert.Equal(dto.ParentCategoryId, model.ParentCategoryId);
             Assert.Equal(dto.ParentCategoryName, model.ParentCategoryName);

             Assert.NotNull(model.Products);
             var dtoProducts = dto.Products!.ToList();
             var modelProducts = model.Products!.ToList();
             Assert.Equal(dtoProducts.Count, modelProducts.Count);
             Assert.Equal(dtoProducts[0].Id, modelProducts[0].Id);
             Assert.Equal(dtoProducts[0].Name, modelProducts[0].Name);
             Assert.Equal(dtoProducts[0].Description, modelProducts[0].Description);
             Assert.Equal(dtoProducts[0].URL, modelProducts[0].URL);
             Assert.Equal(dtoProducts[0].CategoryName, modelProducts[0].CategoryName);
             Assert.Equal(dtoProducts[0].Price, modelProducts[0].Price);
             Assert.Equal(dtoProducts[0].Amount, modelProducts[0].Amount);
         }

         [Fact]
         public void ProductDTO_Maps_To_ProductModel()
         {
             var dto = new ProductDTO
             {
                 Id = Guid.NewGuid(),
                 Name = "Prod",
                 Description = "Desc",
                 URL = "https://prod",
                 CategoryName = "Cat",
                 Price =12.34m,
                 Amount =9,
                 CategoryId = Guid.NewGuid()
             };

             var model = _mapper.Map<ProductModel>(dto);

             Assert.Equal(dto.Id, model.Id);
             Assert.Equal(dto.Name, model.Name);
             Assert.Equal(dto.Description, model.Description);
             Assert.Equal(dto.URL, model.URL);
             Assert.Equal(dto.CategoryName, model.CategoryName);
             Assert.Equal(dto.Price, model.Price);
             Assert.Equal(dto.Amount, model.Amount);
         }
     }
}
