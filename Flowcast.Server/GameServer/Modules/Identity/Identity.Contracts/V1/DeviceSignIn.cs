namespace Identity.Contracts.V1;

// Sign in by DEVICE (recoverable until linked)
public static class DeviceSignIn
{
    public const string Method = "POST";
    public const string Route = "identity/device";

    public const string Summary = "Sign in by device";
    public const string Description = "Creates or reuses an account tied to a device ID. Disables device login after linking to a provider.";

    public record Request(
            string DeviceId,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );
}