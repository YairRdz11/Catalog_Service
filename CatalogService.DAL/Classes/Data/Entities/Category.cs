using System.ComponentModel.DataAnnotations;

namespace CatalogService.DAL.Classes.Data.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Made nullable
        public string? Description { get; set; }

        // Made nullable
        public string? URL { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
