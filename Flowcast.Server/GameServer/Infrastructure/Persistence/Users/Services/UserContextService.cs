using Domain.Users.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Infrastructure.Persistence.Users.Services;

public class UserContextService : IUserContextService
{
    public Guid? GetUserId(HubCallerContext context)
    {
        // 1. Check if the UserId is available in the JWT Token
        var userIdFromJwt = GetUserIdFromJwt(context.User!);
        if (userIdFromJwt != null)
            return userIdFromJwt;

        // 2. Check if the UserId is available in the query string (fallback)
        var userIdFromQuery = GetUserIdFromQueryString(context);
        if (userIdFromQuery != null)
            return userIdFromQuery;

        // 3. Check if the UserId is available in cookies (fallback)
        var userIdFromCookies = GetUserIdFromCookies(context);
        if (userIdFromCookies != null)
            return userIdFromCookies;

        return null;
    }

    private static Guid? GetUserIdFromJwt(ClaimsPrincipal user)
    {
        if (user == null) return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    private static Guid? GetUserIdFromQueryString(HubCallerContext context)
    {
        var userIdQuery = context.GetHttpContext()?.Request.Query["userId"];
        return Guid.TryParse(userIdQuery, out var userId) ? userId : null;
    }

    private static Guid? GetUserIdFromCookies(HubCallerContext context)
    {
        var userIdCookie = context.GetHttpContext()?.Request.Cookies["userId"];
        return Guid.TryParse(userIdCookie, out var userId) ? userId : null;
    }
}
