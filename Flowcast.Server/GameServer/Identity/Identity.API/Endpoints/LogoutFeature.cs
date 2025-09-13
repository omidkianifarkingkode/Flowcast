using Identity.API.Services;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Identity.API.Endpoints;

public static class LogoutFeature
{
    public static void Map(WebApplication app)
    {
        app.MapPost(Logout.Route, async (
            Logout.Request request,
            Handler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new Command(request.RefreshToken), ct);
            return result.Match(
                _ => Results.Ok(new Logout.Response(true)),
                CustomResults.Problem
            );
        })
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    public sealed record Command(string RefreshToken);

    public sealed class Handler(ITokenService tokens)
    {
        public async Task<Result<bool>> Handle(Command command, CancellationToken ct)
        {
            var ok = await tokens.RevokeAsync(command.RefreshToken, ct);
            return ok ? true : Result.Failure<bool>(Error.Problem("Auth.NotRevoked", "Could not revoke refresh token."));
        }
    }
}
