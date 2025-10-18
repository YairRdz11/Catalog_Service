namespace CatalogService.Transversal.Classes.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
        public string CategoryName { get; set; }
    }
}
