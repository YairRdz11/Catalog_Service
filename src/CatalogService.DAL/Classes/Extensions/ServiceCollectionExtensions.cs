using CatalogService.DAL.Classes.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.DAL.Classes.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogData(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CatalogBDContext>(opt => opt.UseSqlServer(connectionString));
            return services;
        }
    }
}
