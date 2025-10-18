namespace CatalogService.Transversal.Classes.Dtos
{
    public class ProductDTO : BaseDTO
    {
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
        public string? URL { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
