using System;
using System.Net;
using System.Text.Json;

namespace KonferenscentrumVast.Exceptions
{
    /// <summary>
    /// Global exception handling middleware that converts exceptions into standardized HTTP responses.
    /// Maps different exception types to appropriate HTTP status codes and returns RFC 7807 Problem Details format.
    /// "RFC" just means it's an official internet standard (Request For Comments), and this middleware creates this format automatically
    /// This centralizes error handling across the entire API.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts all unhandled exceptions in the request pipeline and:
    /// 1. Maps exception types to HTTP status codes (ValidationException -> 400, NotFoundException -> 404, etc.)
    /// 2. Returns consistent JSON error responses following RFC 7807 Problem Details standard
    /// 3. Includes stack traces only in Development environment for security
    /// 4. Logs exceptions appropriately (4xx as Warning, 5xx as Error)
    /// 
    /// Benefits of centralized exception handling:
    /// - Consistent error responses across all endpoints
    /// - Prevents sensitive information leakage in production
    /// - Reduces boilerplate try-catch blocks in controllers
    /// - Makes API responses predictable for client applications
    /// </remarks>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        /// <summary>
        /// Maps exceptions to HTTP status codes and creates RFC 7807 Problem Details responses.
        /// Uses pattern matching to determine appropriate HTTP status and error type.
        /// </summary>
        private async Task HandleAsync(HttpContext ctx, Exception ex)
        {
            var (status, type, title) = ex switch
            {
                ValidationException => (HttpStatusCode.BadRequest, "validation_error", "Validation failed"),
                NotFoundException => (HttpStatusCode.NotFound, "not_found", "Resource not found"),
                ConflictException => (HttpStatusCode.Conflict, "conflict", "Conflict"),
                ArgumentNullException => (HttpStatusCode.BadRequest, "argument_error", "Invalid or missing argument"),
                ArgumentException => (HttpStatusCode.BadRequest, "argument_error", "Invalid argument"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "not_found", "Resource not found"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "unauthorized", "Unauthorized"),
                _ => (HttpStatusCode.InternalServerError, "server_error", "Unexpected error")
            };

            // log: 4xx as Warning, 5xx as Error
            if ((int)status >= 500)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning(ex, "Handled exception: {Type}", type);

            var problem = new
            {
                type,
                title,
                status = (int)status,
                detail = ex.Message,
                traceId = ctx.TraceIdentifier,
                // only include stack in Development
                stack = _env.IsDevelopment() ? ex.StackTrace : null
            };

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = (int)status;

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await ctx.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Extension methods for registering the exception handling middleware in the application pipeline.
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Registers the exception handling middleware. Should be called early in the pipeline
        /// to catch exceptions from all subsequent middleware and controllers.
        /// </summary>
        public static IApplicationBuilder UseExceptionMapping(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
