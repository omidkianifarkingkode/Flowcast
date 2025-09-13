namespace Identity.Contracts.V1;

// Sign in by DEVICE (recoverable until linked)
public static class DeviceSignIn
{
    public const string Method = "POST";
    public const string Route = "auth/device";

    public record Request(
            string DeviceId,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAtUtc
    );
}