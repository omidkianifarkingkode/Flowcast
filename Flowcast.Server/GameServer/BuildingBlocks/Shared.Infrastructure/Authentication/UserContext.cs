using Microsoft.AspNetCore.Http;
using Shared.Application.Authentication;
using System.Security.Claims;

namespace Shared.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Gets the current user ID from the current HTTP context or throws if unavailable
    public Guid UserId => GetUserId(_httpContextAccessor.HttpContext)
                          ?? throw new ApplicationException("User context is unavailable");

    // Try to get the user ID from any HttpContext (for flexibility/testing)
    public Guid? GetUserId(HttpContext? context)
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

    private static Guid? GetUserIdFromJwt(ClaimsPrincipal? user)
    {
        if (user == null) return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id) ? id : null;
    }

    private static Guid? GetUserIdFromQueryString(HttpContext context)
    {
        var userIdQuery = context.Request.Query["userId"];
        return Guid.TryParse(userIdQuery, out var id) ? id : null;
    }

    private static Guid? GetUserIdFromCookies(HttpContext context)
    {
        var userIdCookie = context.Request.Cookies["userId"];
        return Guid.TryParse(userIdCookie, out var id) ? id : null;
    }
}
