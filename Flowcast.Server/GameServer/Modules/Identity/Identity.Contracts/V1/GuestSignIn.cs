using Identity.Contracts.V1.Shared;

namespace Identity.Contracts.V1;

public static class GuestSignIn
{
    public const string Method = "POST";
    public const string Route = "identity/guest";

    public const string Summary = "Sign in as guest";
    public const string Description = "Creates or reuses a guest account; no identity provider required.";

    public record Request(IReadOnlyList<MetadataItem>? Metadata = null);

    //Response 
    public record Response(
       Guid AccountId,
       string AccessToken,
       string RefreshToken,
       DateTimeOffset ExpiresAtUtc
   );

}
