using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass the request through to the next middleware/controller
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full error on the server
            _logger.LogError(ex, "An unhandled exception occurred.");

            // Return a clean error response to the client
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        // Decide status code based on exception type
        context.Response.StatusCode = ex switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,           // 404
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            ArgumentException => (int)HttpStatusCode.BadRequest,             // 400
            _ => (int)HttpStatusCode.InternalServerError                     // 500
        };

        var response = new
        {
            status = context.Response.StatusCode,
            message = ex.Message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}