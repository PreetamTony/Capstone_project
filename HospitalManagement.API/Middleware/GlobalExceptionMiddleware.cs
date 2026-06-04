using System.Net;
using System.Text.Json;
using HospitalManagement.DataAccess.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using UnauthorizedAccessException = HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException;

namespace HospitalManagement.Presentation.Middleware;

/// <summary>
/// Global exception handler middleware. Intercepts all unhandled exceptions and returns
/// a consistent JSON error response. Logs full details server-side while hiding internals from clients.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        var (statusCode, message, errorCode) = exception switch
        {
            NotFoundException nfe =>
                (HttpStatusCode.NotFound, nfe.Message, "NOT_FOUND"),

            BusinessRuleViolationException brve =>
                (HttpStatusCode.BadRequest, brve.Message, brve.Rule),

            UnauthorizedAccessException uae =>
                (HttpStatusCode.Unauthorized, uae.Message, "UNAUTHORIZED"),

            ConcurrencyException ce =>
                (HttpStatusCode.Conflict, ce.Message, "CONCURRENCY_CONFLICT"),

            FluentValidation.ValidationException ve =>
                (HttpStatusCode.BadRequest,
                 string.Join("; ", ve.Errors.Select(e => e.ErrorMessage)),
                 "VALIDATION_ERROR"),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.",
                  "INTERNAL_ERROR")
        };

        // Log with appropriate severity
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        else if (statusCode == HttpStatusCode.BadRequest)
            _logger.LogWarning(exception, "Business rule violation: {Message}", exception.Message);
        else
            _logger.LogInformation(exception, "Handled exception [{Code}]: {Message}", errorCode, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = errorCode,
            Detail = message,
            Instance = context.Request.Path.Value,
        };

        problemDetails.Extensions["success"] = false;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }
}
