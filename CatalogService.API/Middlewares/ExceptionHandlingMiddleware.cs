using CatalogService.Transversal.Classes.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Utilities.Classes.Common;

namespace CatalogService.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            Guid errorId = Guid.Empty;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                errorId = Guid.NewGuid();
                _logger.LogError(ex, "An unhandled exception occurred. ErrorId: {ErrorId}", errorId);
                await WriteErrorResponseAsync(context, ex, errorId);
            }
        }

        private async Task WriteErrorResponseAsync(HttpContext context, Exception ex, Guid errorId)
        {
            int statusCode = MapStatusCode(ex);
            var apiResponse = new ApiResponse
            {
                Status = statusCode,
                ApiError = new ApiError(errorId)
                {
                    Status = (short)statusCode,
                    Title = GetTitle(ex),
                    Detail = ex.Message
                }
            };

            if (ex is ValidateException ve)
            {
                apiResponse.RuleErrors = ve.Errors;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }

        private static int MapStatusCode(Exception ex) =>
        ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTitle(Exception ex) =>
       ex switch
       {
           ValidationException => "Validation Error",
           NotFoundException => "Resource Not Found",
           UnauthorizedAccessException => "Unauthorized",
           DomainException => "Domain Error",
           _ => "Unexpected Error"
       };
    }
}
