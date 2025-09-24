using Identity.Contracts.V1.Shared;

namespace Identity.Contracts.V1;

// Link current account to a provider (disables device login for this account if Provider!=Device)
public static class Link
{
    public const string Method = "POST";
    public const string Route = "identity/link";

    public const string Summary = "Link current account to a provider";
    public const string Description = "Links the authenticated account to a social provider. Disables device login when linked.";

    public record Request(
            IdentityProvider Provider,
            string IdToken,
            string? DisplayName = null,
            Dictionary<string, string>? Meta = null // (capture device/os/app/lang/etc.)
        );

    public record Response(
        Guid AccountId,
        bool Linked
    );
}
