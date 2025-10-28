using Asp.Versioning;
using CatalogService.BLL.Classes;
using CatalogService.DAL.Classes.Extensions;
using CatalogService.DAL.Classes.Mapping;
using CatalogService.DAL.Classes.Repositories;
using CatalogService.Transversal.Interfaces.BL;
using CatalogService.Transversal.Interfaces.DAL;
using CatalogService.Transversal.Mappings;
using Common.ApiUtilities.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add automapper configuration
builder.Services.AddAutoMapper(typeof(MappingProfile), typeof(EntityMappingProfile));

// Add DbContext configuration
var connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddCatalogData(connectionstring);

// Add Service Layer and Repository Layer configurations
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Catalog API", Version = "v1" });
});

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.ReportApiVersions = true;
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

app.UseGlobalExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Project API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
