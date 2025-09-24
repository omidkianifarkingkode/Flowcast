using Identity.Contracts.V1.Shared;

namespace Identity.Contracts.V1;

// 6) Get current account profile
public static class GetProfile
{
    public const string Method = "GET";
    public const string Route = "account/profile";

    public const string Summary = "Get current account profile";
    public const string Description = "Returns profile info and linked identities for the authenticated account.";

    public record Request();

    public record Response(
        Guid AccountId,
        string? DisplayName,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? LastLoginAtUtc,
        string? LastLoginRegion,
        Dtos.IdentitySummary[] Identities
    );
}
