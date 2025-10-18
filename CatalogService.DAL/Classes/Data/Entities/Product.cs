using System.ComponentModel.DataAnnotations;

namespace CatalogService.DAL.Classes.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
