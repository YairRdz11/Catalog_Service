using System.ComponentModel.DataAnnotations;

namespace CatalogService.Transversal.Classes.Models
{
    public class CreateCategoryModel
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? URL { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}
