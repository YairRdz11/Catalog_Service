using Common.Utilities.Classes.Common;

namespace CatalogService.Transversal.Classes.Filters
{
    public class ProductFilterParams : PaginationParams
    {
        public Guid? CategoryId { get; set; }
    }
}
