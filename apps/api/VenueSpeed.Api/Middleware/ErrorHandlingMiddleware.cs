using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        var traceId = Guid.NewGuid();
        var (statusCode, errorCode, message) = MapException(exception);

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        else
            _logger.LogWarning(exception, "Handled exception {ErrorCode}. TraceId: {TraceId}", errorCode, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(message, errorCode, traceId);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (int StatusCode, string Code, string Message) MapException(Exception ex)
        => ex switch
        {
            UnauthorizedAccessException => (401, "UNAUTHORIZED", "Authentication required."),
            KeyNotFoundException => (404, "NOT_FOUND", ex.Message),
            ArgumentException e => (400, "VALIDATION_ERROR", e.Message),
            InvalidOperationException e => (409, "CONFLICT", e.Message),
            SqlException sql => MapSqlException(sql),
            _ => (500, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

    private static (int, string, string) MapSqlException(SqlException ex)
        => ex.Number switch
        {
            2627 or 2601 => (409, "DUPLICATE_RECORD", "A record with that value already exists."),
            547            => (409, "CONSTRAINT_VIOLATION", "Operation violates a data constraint."),
            _              => (500, "DATABASE_ERROR", "A database error occurred.")
        };
}
