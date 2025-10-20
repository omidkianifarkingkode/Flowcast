using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Shared.Presentation.Loggings;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "Correlation-Id";
    private const string CorrelationPropertyName = "CorrelationId";

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);

        using (LogContext.PushProperty(CorrelationPropertyName, correlationId))
        {
            await next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context) =>
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues values)
            ? values.ToString()
            : context.TraceIdentifier;
}
