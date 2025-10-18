using AutoMapper;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Models;

namespace CatalogService.Transversal.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from CategoryDTO to CategoryModel
            CreateMap<CategoryDTO, CategoryModel>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategoryName));

            // Map from CategoryModel to CategoryDTO
            CreateMap<CategoryModel, CategoryDTO>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategoryName));

            // Map from CreateCategoryModel to CategoryDTO
            CreateMap<CreateCategoryModel, CategoryDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId));

            // Map from CategoryDTO to CategoryModel
            CreateMap<ProductDTO, ProductModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));

            // Map from CategoryModel to CategoryDTO
            CreateMap<ProductModel, ProductDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));
        }
    }
}