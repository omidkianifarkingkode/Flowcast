using Identity.API.Infrastructures;
using Identity.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.API.Endpoints;

internal static class JwksEndpoint
{
    public static IEndpointRouteBuilder MapJwksEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/.well-known/jwks.json",
            async (IKeyStore keys, HttpContext http, CancellationToken ct) =>
            {
                var set = await keys.GetValidationSetAsync(ct);
                var jwks = new { keys = set.Select(JwkBuilder.ToJwk).ToArray() };

                // optional: cache headers so clients don’t hammer your service
                http.Response.Headers.CacheControl = "public,max-age=300";

                return Results.Json(jwks);
            })
           .WithTags("JWKS").AllowAnonymous().MapToApiVersion(1.0)
           .Produces(StatusCodes.Status200OK, typeof(object), "application/json");

        return routes;
    }
}
