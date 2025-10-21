using System;

namespace CatalogService.Transversal.Interfaces.Dto
{
    public interface ICategoryDTO
    {
        Guid CategoryId { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string URL { get; set; }
        Guid ParentCategoryId { get; set; }
        string ParentCategoryName { get; set; }
    }
}