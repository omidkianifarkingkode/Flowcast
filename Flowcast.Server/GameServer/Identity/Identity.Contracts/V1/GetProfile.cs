namespace Identity.Contracts.V1;

// 6) Get current account profile
public static class GetProfile
{
    public const string Method = "GET";
    public const string Route = "account/profile";

    public record Request();

    public record Response(
        Guid AccountId,
        string? DisplayName,
        DateTime CreatedAtUtc,
        DateTime? LastLoginAtUtc,
        string? LastLoginRegion,
        Dtos.IdentitySummary[] Identities
    );
}
