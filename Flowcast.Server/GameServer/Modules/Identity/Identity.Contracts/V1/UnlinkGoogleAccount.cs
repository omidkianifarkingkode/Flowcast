namespace Identity.Contracts.V1;

public sealed class UnlinkGoogleAccount
{
    public const string Method = "DELETE";
    public const string Route = "identity/providers/google";

    public const string Summary = "Unlink Google Account";
    public const string Description = "Remove the link between the current account and Google provider.";
}
