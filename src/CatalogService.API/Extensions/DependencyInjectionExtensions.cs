using Asp.Versioning;
using CatalogService.BLL.Classes;
using CatalogService.DAL.Classes.Extensions;
using CatalogService.DAL.Classes.Mapping;
using CatalogService.DAL.Classes.Repositories;
using CatalogService.DAL.Messaging;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using CatalogService.Transversal.Mappings;
using Common.Utilities.Classes.Messaging.Options;
using Common.Utilities.Classes.Messaging.Publisher;
using Common.Utilities.Interfaces.Messaging.Events;

namespace CatalogService.API.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile), typeof(EntityMappingProfile));

            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("The 'DefaultConnection' connection string is missing from configuration.");
            services.AddCatalogData(connectionString);

            // Repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOutboxWriter, OutboxWriter>();

            // Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();

            // HTTP Client
            services.AddHttpClient();

            // RabbitMQ
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHostedService<OutboxProcessorHostedService>();

            return services;
        }

        public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
        {
            var apiVersioningBuilder = services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            apiVersioningBuilder.AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
