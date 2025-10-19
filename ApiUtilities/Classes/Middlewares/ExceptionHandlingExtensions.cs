using Microsoft.AspNetCore.Builder;

namespace CatalogService.API.Middlewares
{
    public static class ExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
       => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
