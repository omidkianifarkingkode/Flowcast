using Identity.API.Services;
using SharedKernel;

namespace Identity.API.Businesses.Commands;

public sealed record LogoutCommand(string RefreshToken);

public sealed class LogoutCommandHandler(ITokenService tokens)
{
    public async Task<Result<bool>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var ok = await tokens.RevokeAsync(command.RefreshToken, ct);
        return ok
            ? Result.Success(true)
            : Result.Failure<bool>(Error.Problem("Auth.NotRevoked", "Could not revoke refresh token."));
    }
}
