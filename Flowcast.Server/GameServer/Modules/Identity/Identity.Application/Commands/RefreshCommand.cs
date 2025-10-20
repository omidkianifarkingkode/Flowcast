using Identity.Application.Services;
using Shared.Application.Messaging;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record RefreshCommand(string RefreshToken) : ICommand<AuthResult>;

public sealed class RefreshCommandHandler(ITokenService tokens) : ICommandHandler<RefreshCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(RefreshCommand command, CancellationToken ct)
    {
        var (ok, accountId, access, refresh, exp) = await tokens.RefreshAsync(command.RefreshToken, ct);
        if (!ok)
            return Result.Failure<AuthResult>(
                Error.Unauthorized("Auth.InvalidRefresh", "Refresh token is invalid or expired."));

        return new AuthResult(accountId, access, refresh, exp);
    }
}
