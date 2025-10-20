using SharedKernel;

namespace Identity.Domain.Shared;

public static class ProviderErrors
{
    public static readonly Error UnsupportedProvider =
        Error.Failure("Provider.Unsupported", "The identity provider is not supported.");

    public static readonly Error InvalidNonce =
        Error.Failure("Provider.InvalidNonce", "The ID token nonce does not match the expected value.");

    public static readonly Error EmailNotVerified =
        Error.Unauthorized("Provider.EmailNotVerified", "The user's email address is not verified.");

    public static Error Misconfigured(string what) =>
        Error.Problem("Provider.Misconfigured", $"Provider is misconfigured: {what}");

    public static Error InvalidToken(string reason) =>
        Error.Unauthorized("Provider.InvalidToken", $"Invalid ID token: {reason}");

    public static readonly Error Unknown =
        Error.Problem("Provider.Unknown", "Unknown provider verification error.");
}
