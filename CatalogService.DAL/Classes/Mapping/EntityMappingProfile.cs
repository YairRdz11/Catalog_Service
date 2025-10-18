using AutoMapper;
using CatalogService.DAL.Classes.Data.Entities;
using CatalogService.Transversal.Classes.Dtos;

namespace CatalogService.DAL.Classes.Mapping
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<Category, CategoryDTO>()
                .ForMember(d => d.CategoryId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.ParentCategoryId, m => m.MapFrom(s => s.ParentCategoryId ?? Guid.Empty))
                .ForMember(d => d.ParentCategoryName, m => m.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));

            CreateMap<CategoryDTO, Category>()
                .ForMember(e => e.Id, m => m.MapFrom(d => d.CategoryId))
                .ForMember(e => e.ParentCategoryId, m => m.MapFrom(d => d.ParentCategoryId == Guid.Empty ? (Guid?)null : d.ParentCategoryId))
                .ForMember(e => e.ParentCategory, m => m.Ignore())
                .ForMember(e => e.Products, m => m.Ignore());


            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.ProductId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.CategoryId, m => m.MapFrom(s => s.CategoryId))
                .ForMember(d => d.CategoryName, m => m.MapFrom(s => s.Category.Name != null ? s.Category.Name : null))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description))
                .ForMember(d => d.URL, m => m.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.Price, m => m.MapFrom(s => s.Price))
                .ForMember(d => d.Amount, m => m.MapFrom(s => s.Amount));

            CreateMap<ProductDTO, Product>()
                .ForMember(d => d.Id, m => m.MapFrom(s => s.ProductId))
                .ForMember(d => d.CategoryId, m => m.MapFrom(s => s.CategoryId))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description))
                .ForMember(d => d.ImageUrl, m => m.MapFrom(s => s.URL))
                .ForMember(d => d.Price, m => m.MapFrom(s => s.Price))
                .ForMember(d => d.Amount, m => m.MapFrom(s => s.Amount));
        }
    }
}
