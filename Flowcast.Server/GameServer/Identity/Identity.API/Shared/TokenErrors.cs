using SharedKernel;

namespace Identity.API.Shared;

public static class TokenErrors
{
    public static readonly Error InvalidFormat =
        Error.Failure("Token.InvalidFormat", "The token format is invalid.");

    public static readonly Error InvalidAlgorithm =
        Error.Failure("Token.InvalidAlgorithm", "The token algorithm is not supported or mismatched.");

    public static readonly Error WrongTokenType =
        Error.Failure("Token.WrongType", "The token type does not match the expected value.");

    public static readonly Error Expired =
        Error.Unauthorized("Token.Expired", "The token has expired.");

    public static readonly Error InvalidSignatureOrClaims =
        Error.Unauthorized("Token.InvalidSignatureOrClaims", "The token signature or claims are invalid.");
}