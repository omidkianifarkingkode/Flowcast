namespace Identity.Contracts.V1;

// Refresh tokens (rotating refresh token model)
public static class Refresh
{
    public const string Method = "POST";
    public const string Route = "/identity/refresh";

    public const string Summary = "Refresh access/refresh tokens";
    public const string Description = "Validates the provided refresh token and issues a new access/refresh token pair.";

    public record Request(string RefreshToken);

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );
}