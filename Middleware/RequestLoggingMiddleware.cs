using System.Diagnostics;
using System.Text.Json;

namespace StoreApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    // Paths to skip logging (health checks, swagger, etc.)
    private static readonly HashSet<string> SkipPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/healthz",
        "/swagger",
        "/favicon.ico"
    };
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value ?? "";
        var requestMethod = context.Request.Method;
        
        // Skip logging for certain paths
        if (ShouldSkipLogging(requestPath))
        {
            await _next(context);
            return;
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;
            
            // Only log slow requests (>500ms) or errors at Information level
            // Log successful fast requests at Debug level
            if (statusCode >= 400 || elapsed > 500)
            {
                _logger.LogWarning(
                    "{Method} {Path} responded {StatusCode} in {Duration}ms",
                    requestMethod, requestPath, statusCode, elapsed);
            }
            else
            {
                _logger.LogDebug(
                    "{Method} {Path} responded {StatusCode} in {Duration}ms",
                    requestMethod, requestPath, statusCode, elapsed);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await HandleExceptionAsync(context, ex, requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
        }
    }
    
    private static bool ShouldSkipLogging(string path)
    {
        // Skip swagger UI resources
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return true;
            
        return SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string method, string path, long elapsed)
    {
        // Log once at the boundary
        _logger.LogError(exception,
            "Unhandled exception during {Method} {Path} after {Duration}ms",
            method, path, elapsed);
        
        // Determine status code based on exception type
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        
        // Return ProblemDetails response (standard format)
        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = GetTitleForStatusCode(statusCode),
            status = statusCode,
            detail = IsDevelopment(context) ? exception.Message : "An error occurred processing your request.",
            traceId = context.TraceIdentifier
        };
        
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
    
    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "Error"
    };
    
    private static bool IsDevelopment(HttpContext context)
    {
        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        return env?.IsDevelopment() ?? false;
    }
}
