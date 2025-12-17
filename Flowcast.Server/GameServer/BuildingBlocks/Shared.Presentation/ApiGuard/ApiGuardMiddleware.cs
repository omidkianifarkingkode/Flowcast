using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Shared.Presentation.ApiGuard;

internal sealed class ApiGuardMiddleware(IOptions<ApiGuardOptions> opt, IHostEnvironment env) : IMiddleware
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private readonly ApiGuardOptions _opt = opt.Value;
    private readonly IHostEnvironment _env = env;

    public Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        if (!_opt.Enabled)
            return next(ctx);

        var endpoint = ctx.GetEndpoint();
        if (endpoint is null)
            return next(ctx);

        var currentMode = _env.EnvironmentName;

        // 1) Allow-only has highest priority
        var allow = endpoint.Metadata.GetMetadata<AllowOnlyLaunchModes>();
        if (allow is not null)
        {
            if (!Contains(allow.Modes, currentMode))
                return Block(ctx, currentMode, allow.Modes, "allow-only");

            return next(ctx);
        }

        // 2) Block list
        var block = endpoint.Metadata.GetMetadata<BlockLaunchModes>();
        if (block is not null)
        {
            if (Contains(block.Modes, currentMode))
                return Block(ctx, currentMode, block.Modes, "blocked");

            return next(ctx);
        }

        return next(ctx);
    }

    private static bool Contains(string[] modes, string current)
    {
        for (int i = 0; i < modes.Length; i++)
        {
            if (Comparer.Equals(modes[i], current))
                return true;
        }
        return false;
    }

    private static Task Block(
        HttpContext ctx,
        string currentMode,
        string[] modes,
        string rule)
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        ctx.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = "https://httpstatuses.com/403",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = "This endpoint is not allowed in the current launch mode.",
            Instance = ctx.Request.GetDisplayUrl()
        };

        // Helpful but not leaking sensitive info
        problem.Extensions["launchMode"] = currentMode;
        problem.Extensions["rule"] = rule;
        problem.Extensions["modes"] = modes;

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
