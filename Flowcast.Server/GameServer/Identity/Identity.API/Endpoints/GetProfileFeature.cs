// /Identity/V1/GetProfileFeature.cs
using Identity.API.Extensions;
using Identity.API.Repositories;
using Identity.API.Shared;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using static Identity.API.Endpoints.GetProfileFeature.ResultDto;

namespace Identity.API.Endpoints;

public static class GetProfileFeature
{
    public static void Map(WebApplication app)
    {
        app.MapGet(GetProfile.Route, async (
            HttpContext http,
            Handler handler,
            CancellationToken ct) =>
        {
            var aid = http.GetAccountId();
            var result = await handler.Handle(new Query(aid), ct);

            return result.Match(
                ok => Results.Ok(ToResponse(ok)),
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    // -------- Query + Handler --------
    public sealed record Query(Guid AccountId);

    public sealed record ResultDto(
        Guid AccountId, string? DisplayName, DateTime CreatedAtUtc,
        DateTime? LastLoginAtUtc, string? LastLoginRegion,
        IReadOnlyList<IdentityRow> Identities)
    {
        public sealed record IdentityRow(
            Guid IdentityId, string Provider, string SubjectMasked, bool LoginAllowed,
            DateTime CreatedAtUtc, DateTime? LastSeenAtUtc, Dictionary<string, string>? LastMeta);
    }

    public sealed class Handler(IAccountRepository accounts, IIdentityRepository identities)
    {
        public async Task<Result<ResultDto>> Handle(Query query, CancellationToken ct)
        {
            var acc = await accounts.GetById(query.AccountId, ct);
            if (acc is null) return Result.Failure<ResultDto>(Error.DefaultNotFound);

            var ids = await identities.GetByAccountId(query.AccountId, ct);
            static string Mask(string s) => s.Length <= 6 ? "***" : $"{new string('*', s.Length - 4)}{s[^4..]}";

            var rows = ids.Select(x => new IdentityRow(
                x.IdentityId,
                x.Provider.ToString(),
                Mask(x.Subject),
                x.LoginAllowed,
                x.CreatedAtUtc,
                x.LastSeenAtUtc,
                x.LastMeta
            )).ToList();

            return new ResultDto(
                acc.AccountId,
                acc.DisplayName,
                acc.CreatedAtUtc,
                acc.LastLoginAtUtc,
                acc.LastLoginRegion,
                rows);
        }
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
