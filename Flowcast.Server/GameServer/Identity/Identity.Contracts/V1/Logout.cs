namespace Identity.Contracts.V1;

// Logout (revoke current refresh token)
public static class Logout
{
    public const string Method = "POST";
    public const string Route = "auth/logout";

    // If you track multiple refresh tokens per device, include the exact one to revoke.
    public record Request(string RefreshToken);

    public record Response(bool LoggedOut);
}
