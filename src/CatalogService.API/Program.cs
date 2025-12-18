using CatalogService.API.Extensions;
using Common.ApiUtilities.Middleware;
using Common.Utilities.Classes.Extensions;
using Microsoft.EntityFrameworkCore;
using CatalogService.DAL.Classes.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSwaggerDocumentation(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddApiVersioningConfiguration();

var app = builder.Build();

// Apply EF Core migrations only when enabled
var migrateOnStartup = builder.Configuration.GetValue<bool>("Database:MigrateOnStartup");
if (migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogBDContext>();
    await db.Database.MigrateAsync();
}

app.UseGlobalExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

await app.RunAsync();
