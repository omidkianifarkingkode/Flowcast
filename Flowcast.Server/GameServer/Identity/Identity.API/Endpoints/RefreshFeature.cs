// /Identity/V1/RefreshFeature.cs
using Identity.API.Services;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Identity.API.Endpoints;

public static class RefreshFeature
{
    public static void Map(WebApplication app)
    {
        app.MapPost(Refresh.Route, async (
            Refresh.Request request,
            Handler handler,
            CancellationToken ct) =>
        {
            var cmd = new Command(request.RefreshToken);
            var result = await handler.Handle(cmd, ct);

            return result.Match(
                ok => Results.Ok(new Refresh.Response(ok.AccountId, ok.AccessToken, ok.RefreshToken, ok.ExpiresAtUtc)),
                CustomResults.Problem
            );
        })
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    public sealed record Command(string RefreshToken);

    public sealed class Handler(ITokenService tokens)
    {
        public async Task<Result<DeviceSignInFeature.AuthResult>> Handle(Command command, CancellationToken ct)
        {
            var (ok, accountId, access, refresh, exp) = await tokens.RefreshAsync(command.RefreshToken, ct);
            if (!ok)
                return Result.Failure<DeviceSignInFeature.AuthResult>(
                    Error.Unauthorized("Auth.InvalidRefresh", "Refresh token is invalid or expired."));

            return new DeviceSignInFeature.AuthResult(accountId, access, refresh, exp);
        }
    }
}
