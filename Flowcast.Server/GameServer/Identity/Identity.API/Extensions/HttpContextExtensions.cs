using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Identity.API.Extensions;

internal static class HttpContextExtensions
{
    public static Guid GetAccountId(this HttpContext http)
    {
        // Prefer "aid" claim, fallback to NameIdentifier
        var str = http.User.FindFirst("aid")?.Value
               ?? http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(str))
            throw new InvalidOperationException("Authenticated user does not contain an account id claim.");

        if (!Guid.TryParse(str, out var accountId))
            throw new InvalidOperationException("Account id claim is not a valid GUID.");

        return accountId;
    }
}
