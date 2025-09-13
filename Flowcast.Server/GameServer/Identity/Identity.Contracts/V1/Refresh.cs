namespace Identity.Contracts.V1;

// 4) Refresh tokens (rotating refresh token model)
public static class Refresh
{
    public const string Method = "POST";
    public const string Route = "auth/refresh";

    public record Request(string RefreshToken);

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAtUtc
    );
}
