using Identity.Application.Services;
using Shared.Application.Messaging;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

public sealed class LogoutCommandHandler(ITokenService tokens) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct)
    {
        var ok = await tokens.RevokeAsync(command.RefreshToken, ct);
        return ok
            ? Result.Success()
            : Result.Failure(Error.Problem("Auth.NotRevoked", "Could not revoke refresh token."));
    }
}
