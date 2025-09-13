namespace Identity.Contracts.V1;

// Sign in with GOOGLE (example provider)
public static class GoogleSignIn
{
    public const string Method = "POST";
    public const string Route = "auth/google";

    public record Request(
            string IdToken,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAtUtc
    );
}
