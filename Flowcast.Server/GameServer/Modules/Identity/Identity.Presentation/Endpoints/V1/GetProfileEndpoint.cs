using Identity.Application.Queries;
using Identity.Contracts;
using Identity.Contracts.V1;
using Identity.Contracts.V1.Shared;
using Identity.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(GetProfile.Route,
            async (IQueryHandler<GetProfileQuery, ResultDto> handler,
                   HttpContext http,
                   CancellationToken ct) =>
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
           .WithTags(ApiInfo.Tag)
           .WithSummary(GetProfile.Summary)
           .WithDescription(GetProfile.Description);
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
