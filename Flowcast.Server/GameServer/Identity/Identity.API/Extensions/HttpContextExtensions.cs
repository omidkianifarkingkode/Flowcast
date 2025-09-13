using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Identity.API.Extensions;

internal static class HttpContextExtensions
{
    public static Guid GetAccountId(this HttpContext http)
        => Guid.Parse(http.User.FindFirst("aid")?.Value
            ?? http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
