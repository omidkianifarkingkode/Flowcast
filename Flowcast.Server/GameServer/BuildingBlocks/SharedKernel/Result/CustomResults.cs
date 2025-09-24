using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace SharedKernel;

public static class CustomResults
{
    // ---- Constants (single source of truth) ----
    private const string TitleServerFailure = "Server failure";
    private const string DetailUnexpected = "An unexpected error occurred";
    private const string KeyErrors = "errors";
    private const string KeyTraceId = "traceId";

    // RFC type URIs (datatracker)
    private const string TypeBadRequest = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
    private const string TypeNotFound = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";
    private const string TypeConflict = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
    private const string TypeUnauthorized = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1";
    private const string TypeServerError = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
    private const string TypeForbidden = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3";

    /// <summary>
    /// Builds a ProblemDetails IResult from a domain Result.
    /// Optionally pass HttpContext to enrich with instance + traceId.
    /// </summary>
    public static IResult Problem(Result result, HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot build a problem from a successful result.");

        var error = result.Error;

        var title = GetTitle(error);
        var detail = GetDetail(error);
        var type = GetType(error.Type);
        var status = GetStatusCode(error.Type);

        var extensions = GetExtensions(result, httpContext);

        return Results.Problem(
            title: title,
            detail: detail,
            type: type,
            statusCode: status,
            instance: httpContext?.Request.Path.Value,
            extensions: extensions
        );
    }

    public static IResult Problem<T>(Result<T> result, HttpContext? httpContext = null)
        => Problem(Result.Failure(result.Error), httpContext);

    public static IResult Problem(Error error, HttpContext? httpContext = null)
        => Problem(Result.Failure(error), httpContext);


    // ---- Local helpers (use constants above) ----

    private static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Code,
            ErrorType.Problem => error.Code,
            ErrorType.NotFound => error.Code,
            ErrorType.Conflict => error.Code,
            ErrorType.Unauthorized => error.Code,
            ErrorType.Forbidden => error.Code,
            _ => TitleServerFailure
        };

    private static string GetDetail(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Description,
            ErrorType.Problem => error.Description,
            ErrorType.NotFound => error.Description,
            ErrorType.Conflict => error.Description,
            ErrorType.Unauthorized => error.Description,
            ErrorType.Forbidden => error.Description,
            _ => DetailUnexpected
        };

    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => TypeBadRequest,
            ErrorType.Problem => TypeBadRequest,
            ErrorType.NotFound => TypeNotFound,
            ErrorType.Conflict => TypeConflict,
            ErrorType.Unauthorized => TypeUnauthorized,
            ErrorType.Forbidden => TypeForbidden,
            _ => TypeServerError
        };

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

    private static Dictionary<string, object?>? GetExtensions(Result result, HttpContext? ctx)
    {
        Dictionary<string, object?>? dict = null;

        // Add validation errors if present
        if (result.Error is ValidationError ve && ve.Errors is not null)
        {
            dict ??= new Dictionary<string, object?>(capacity: 2);
            dict[KeyErrors] = ve.Errors;
        }

        // Add traceId for diagnostics (no PII)
        var traceId = Activity.Current?.Id ?? ctx?.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
        {
            dict ??= new Dictionary<string, object?>(capacity: 2);
            dict[KeyTraceId] = traceId!;
        }

        return dict;
    }
}
