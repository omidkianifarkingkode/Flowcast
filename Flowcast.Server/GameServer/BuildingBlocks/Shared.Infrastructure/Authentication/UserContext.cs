using Microsoft.AspNetCore.Http;
using Shared.Application.Authentication;
using System.Security.Claims;

namespace Shared.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{

    // Gets the current user ID from the current HTTP context or throws if unavailable
    public string UserId => GetUserId(httpContextAccessor.HttpContext)
                          ?? throw new ApplicationException("User context is unavailable");

    // Try to get the user ID from any HttpContext (for flexibility/testing)
    public string? GetUserId(HttpContext? context)
    {
        if (context == null) return null;

        // 1. Try JWT token claims
        var userId = GetUserIdFromJwt(context.User);
        if (userId != null) return userId;

        // 2. Try query string fallback
        userId = GetUserIdFromQueryString(context);
        if (userId != null) return userId;

        // 3. Try cookies fallback
        return GetUserIdFromCookies(context);
    }

    private static string? GetUserIdFromJwt(ClaimsPrincipal? user)
    {
        if (user == null) return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value;
    }

    private static string? GetUserIdFromQueryString(HttpContext context)
    {
        var userIdQuery = context.Request.Query["userId"];
        return userIdQuery;
    }

    private static string? GetUserIdFromCookies(HttpContext context)
    {
        var userIdCookie = context.Request.Cookies["userId"];
        return userIdCookie;
    }
}
