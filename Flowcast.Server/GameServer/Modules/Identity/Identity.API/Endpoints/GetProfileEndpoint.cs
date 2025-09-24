using Identity.API.Businesses.Commands;
using Identity.API.Extensions;
using Identity.Contracts.V1;
using Identity.Contracts.V1.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel;

namespace Identity.API.Endpoints;

internal static class GetProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetProfileEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(GetProfile.Route,
            async (GetProfileQueryHandler handler, HttpContext http, CancellationToken ct) =>
            {
                var accountId = http.GetAccountId();
                if (accountId.IsFailure)
                    return CustomResults.Problem(accountId, http);

                var query = new GetProfileQuery(accountId.Value);
                var result = await handler.Handle(query, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(auth)),
                    error => CustomResults.Problem(error, http));

            })
           //.AllowAnonymous() => requires auth
           .RequireAuthorization()
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(GetProfile.Summary)
           .WithDescription(GetProfile.Description);

        return routes;
    }

    private static GetProfile.Response ToResponse(ResultDto r)
        => new(
            r.AccountId,
            r.DisplayName,
            r.CreatedAtUtc,
            r.LastLoginAtUtc,
            r.LastLoginRegion,
            r.Identities.Select(i => new Dtos.IdentitySummary(
                i.IdentityId,
                i.Provider,
                i.SubjectMasked,
                i.LoginAllowed,
                i.CreatedAtUtc,
                i.LastSeenAtUtc,
                i.LastMeta)).ToArray());
}
