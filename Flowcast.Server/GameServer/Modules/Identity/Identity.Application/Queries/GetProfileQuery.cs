using Identity.Application.Repositories;
using Shared.Application.Messaging;
using SharedKernel;
using static Identity.Application.Queries.ResultDto;

namespace Identity.Application.Queries;

public sealed record GetProfileQuery(Guid AccountId) : IQuery<ResultDto>;

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
    : IQueryHandler<GetProfileQuery, ResultDto>
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
