using Identity.Application.Services;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Presentation.Endpoints;

namespace Identity.Presentation.Endpoints.V1;

public sealed class JwksEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/jwks.json",
            async (IKeyStore keys, HttpContext http, CancellationToken ct) =>
            {
                var set = await keys.GetValidationSetAsync(ct);
                var jwks = new { keys = set.Select(JwkBuilder.ToJwk).ToArray() };

                // optional: cache headers so clients don’t hammer your service
                http.Response.Headers.CacheControl = "public,max-age=300";

                return Results.Json(jwks);
            })
           .WithTags("JWKS")
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .Produces(StatusCodes.Status200OK, typeof(object), "application/json");
    }
}
