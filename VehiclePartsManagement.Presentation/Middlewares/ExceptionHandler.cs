using System.Text.Json;
using VehiclePartsManagement.Application.Common;

namespace VehiclePartsManagement.Presentation.Middlewares;

/// <summary>
/// Catches unhandled exceptions and returns standardized API error responses.
/// </summary>
public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }


    /// <summary>
    /// Middleware entry point.
    /// </summary>
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = ApiResponse.FailureResponse(
                message: "Internal server error",
                errors: new Dictionary<string, List<string>>
                {
                    { "server", ["Something went wrong. Please try again later."] }
                });

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await httpContext.Response.WriteAsync(json);
        }
    }
}
