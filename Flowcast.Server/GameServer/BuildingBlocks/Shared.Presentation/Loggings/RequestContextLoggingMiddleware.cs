using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Shared.API.Loggings;

public class RequestContextLoggingMiddleware
{
    private const string CorrelationIdHeaderName = "Correlation-Id";
    private const string CorrelationPropertyName = "CorrelationId";

    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);

        using (LogContext.PushProperty(CorrelationPropertyName, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context) =>
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues values)
            ? values.ToString()
            : context.TraceIdentifier;
}
