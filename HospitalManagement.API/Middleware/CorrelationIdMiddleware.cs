using HospitalManagement.DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HospitalManagement.Presentation.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId))
        {
            correlationIdAccessor.CorrelationId = correlationId.ToString();
        }
        else
        {
            correlationIdAccessor.CorrelationId = Guid.NewGuid().ToString();
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationIdAccessor.CorrelationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
