using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.API.ErrorHandling;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string UnhandledErrorMessage = "Unhandled exception occurred";
    private const string ContentTypeProblemJson = "application/problem+json";
    private const string TraceIdKey = "traceId";

    private static readonly ProblemDetails Template = new()
    {
        Status = StatusCodes.Status500InternalServerError,
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
        Title = "Server failure"
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, UnhandledErrorMessage);

        var problemDetails = CreateProblemDetails(httpContext);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.ContentType = ContentTypeProblemJson;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext ctx)
    {
        // Make a fresh instance per request, copying the static fields
        var pd = new ProblemDetails
        {
            Status = Template.Status,
            Type = Template.Type,
            Title = Template.Title,
            // RFC 7807 recommends 'instance' be the request path or a unique URI
            Instance = ctx.Request.Path
        };

        // Helpful diagnostics without leaking PII
        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;
        pd.Extensions[TraceIdKey] = traceId;

        return pd;
    }
}
