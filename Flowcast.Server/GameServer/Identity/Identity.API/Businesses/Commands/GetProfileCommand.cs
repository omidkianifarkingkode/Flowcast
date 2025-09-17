// /Identity/V1/GetProfileFeature.cs
using Identity.API.Services.Repositories;
using Identity.Contracts.V1;
using SharedKernel;
using static Identity.API.Businesses.Commands.ResultDto;

namespace Identity.API.Businesses.Commands;

public sealed record GetProfileQuery(Guid AccountId);

public sealed record ResultDto(
    Guid AccountId, string? DisplayName, DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc, string? LastLoginRegion,
    IReadOnlyList<IdentityRow> Identities)
{
    public sealed record IdentityRow(
        Guid IdentityId, string Provider, string SubjectMasked, bool LoginAllowed,
        DateTime CreatedAtUtc, DateTime? LastSeenAtUtc, IReadOnlyDictionary<string, string>? LastMeta);
}

public sealed class GetProfileQueryHandler(IAccountRepository accounts, IIdentityRepository identities)
{
    public async Task<Result<ResultDto>> Handle(GetProfileQuery query, CancellationToken ct)
    {
        var acc = await accounts.GetById(query.AccountId, ct);
        if (acc is null)
            return Result.Failure<ResultDto>(Error.DefaultNotFound);

        var ids = await identities.GetByAccountId(query.AccountId, ct);

        static string Mask(string s)
            => string.IsNullOrEmpty(s) ? "***"
             : s.Length <= 6 ? "***"
             : new string('*', s.Length - 4) + s[^4..];

        var rows = ids.Select(x => new IdentityRow(
                x.IdentityId,
                x.Provider.ToString(),
                Mask(x.Subject),
                x.LoginAllowed,
                x.CreatedAtUtc,
                x.LastSeenAtUtc,
                x.LastMeta))
            .ToList();

        return new ResultDto(
            acc.AccountId,
            acc.DisplayName,
            acc.CreatedAtUtc,
            acc.LastLoginAtUtc,
            acc.LastLoginRegion,
            rows);
    }
}
