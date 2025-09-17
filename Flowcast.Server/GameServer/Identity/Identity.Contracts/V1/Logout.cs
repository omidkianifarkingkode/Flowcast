namespace Identity.Contracts.V1;

// Logout (revoke current refresh token)
public static class Logout
{
    public const string Method = "POST";
    public const string Route = "auth/logout";

    public const string Summary = "Logout (revoke refresh token)";
    public const string Description = "Revokes the provided refresh token. If rotation is enabled, revoke the token family.";

    // If you track multiple refresh tokens per device, include the exact one to revoke.
    public record Request(string RefreshToken);

    public record Response(bool LoggedOut);
}
