using System.Text.Json;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            DomainException domainEx => (StatusCodes.Status400BadRequest, domainEx.Message),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Handled exception: {ExceptionType}", exception.GetType().Name);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new
        {
            type = GetProblemType(statusCode),
            title = GetProblemTitle(statusCode),
            status = statusCode,
            detail = message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static string GetProblemType(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static string GetProblemTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "Error"
    };
}
