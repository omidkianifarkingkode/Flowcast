using Identity.Contracts.V1.Shared;

namespace Identity.Contracts.V1;

public sealed class LinkGoogleAccount
{
    public const string Method = "PUT";
    public const string Route = "identity/providers/google";

    public const string Summary = "Link Google Account";
    public const string Description = "Link current account to a Google user ID (Trust-on-Client).";

    public record Request(
        string GoogleId, 
        string? DisplayName = null,
        IReadOnlyList<MetadataItem>? Metadata = null
        );
}
