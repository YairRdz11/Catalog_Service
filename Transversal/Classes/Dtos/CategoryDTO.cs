using CatalogService.Transversal.Interfaces.Dto;

namespace CatalogService.Transversal.Classes.Dtos
{
    public class CategoryDTO : ICategoryDTO
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public Guid ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
    }
}
