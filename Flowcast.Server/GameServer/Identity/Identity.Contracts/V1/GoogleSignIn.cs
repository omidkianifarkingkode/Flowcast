namespace Identity.Contracts.V1;

// Sign in with GOOGLE (example provider)
public static class GoogleSignIn
{
    public const string Method = "POST";
    public const string Route = "identity/google";

    public const string Summary = "Sign in with Google";
    public const string Description = "Validates Google ID token, signs in existing user or creates a new account, and issues tokens.";

    public record Request(
            string IdToken,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );
}
