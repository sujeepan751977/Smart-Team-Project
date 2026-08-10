using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Recruitment_Project.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
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
                _logger.LogWarning(ex, ex.Message);

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                // Ownership failures are Forbidden; claim/auth failures are Unauthorized.
                var status =
                    ex.Message.Contains("claim", StringComparison.OrdinalIgnoreCase)
                        ? HttpStatusCode.Unauthorized
                        : HttpStatusCode.Forbidden;

                await WriteErrorAsync(
                    context,
                    status,
                    ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "The request conflicts with existing data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        private static async Task WriteErrorAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                Success = false,
                Message = message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
