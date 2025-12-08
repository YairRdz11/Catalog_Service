namespace CatalogService.Transversal.Classes.Dtos
{
    public class CategoryDTO : BaseDTO
    {
        public string Description { get; set; }
        public string URL { get; set; }
        public Guid ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public IEnumerable<ProductDTO> Products { get; set; }
    }
}
