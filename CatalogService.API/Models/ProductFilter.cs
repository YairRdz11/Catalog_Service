using Common.Utilities.Classes.Common;

namespace CatalogService.API.Models
{
    public class ProductFilter : PaginationParams
    {
        public Guid CategoryId { get; set; }
    }
}
