using Identity.Contracts.V1.Shared;

namespace Identity.Contracts.V1;

public static class GoogleSignIn
{
    public const string Method = "POST";
    public const string Route = "identity/google";

    public const string Summary = "Sign in with Google";
    public const string Description = "Sign in or create account by Google user ID; no token validation.";

    public record Request(string UserId, IReadOnlyList<MetadataItem>? Metadata = null);

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );
}
