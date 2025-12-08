using System.ComponentModel.DataAnnotations;

namespace CatalogService.Transversal.Classes.Models
{
    public class CreateProductModel
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? URL { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
    }
}
