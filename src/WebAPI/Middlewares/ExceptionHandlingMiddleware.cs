using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebAPI.Middlewares;

/// <summary>
/// Centralized exception handling middleware to intercept all application exceptions,
/// prevent raw stack trace exposure, and return standardized JSON error responses.
/// </summary>
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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Avoid writing to a response that has already started because they are read only afterwards.
        if (context.Response.HasStarted)
            return;
        
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        object? details = null;

        switch (exception)
        {
            case CustomerNotFoundException or SubscriptionNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
                
            case DuplicatePaymentException:
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
                break;
                
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                details = validationException.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });
                break;
                
            default:
                // For generic exceptions, keep the generic 500 error message to avoid leaking sensitive information
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = statusCode.ToString(),
            message,
            details
        };

        // Avoid string allocation for each response, serialize directly to the response body
        await JsonSerializer.SerializeAsync(context.Response.Body, response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
