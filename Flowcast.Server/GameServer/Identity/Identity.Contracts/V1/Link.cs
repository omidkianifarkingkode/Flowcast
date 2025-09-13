namespace Identity.Contracts.V1;

// Link current account to a provider (disables device login for this account if Provider!=Device)
public static class Link
{
    public const string Method = "POST";
    public const string Route = "auth/link";

    public record Request(
            string Provider, // Google/Facebook/Apple
            string IdToken,
            string? DisplayName = null,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        bool Linked
    );
}
