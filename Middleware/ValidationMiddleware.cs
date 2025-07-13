using FluentValidation;
using PengerAPI.DTOs;
using System.Net;
using System.Text.Json;

namespace PengerAPI.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error occurred: {Message}", ex.Message);
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleGenericExceptionAsync(context, ex);
            }
        }

        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = ex.Errors.Select(error => new ValidationErrorResponse
            {
                Field = error.PropertyName,
                Message = error.ErrorMessage,
                AttemptedValue = error.AttemptedValue
            }).ToList();

            var response = ApiResponse<object>.ErrorResult(
                "Validation failed",
                errors.Select(e => $"{e.Field}: {e.Message}").ToList()
            );

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse<object>.ErrorResult(
                "An error occurred while processing your request",
                new List<string> { ex.Message }
            );

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension method for easy middleware registration
    public static class ValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}
