using Identity.API.Entities;
using Identity.API.Repositories;
using Identity.API.Services;
using Identity.API.Shared;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Identity.API.Endpoints;

public static class GoogleSignInFeature
{
    public static void Map(WebApplication app)
    {
        app.MapPost(GoogleSignIn.Route, async (
            GoogleSignIn.Request request,
            Handler handler,
            CancellationToken ct) =>
        {
            var cmd = new Command(IdentityProvider.Google, request.IdToken, request.Meta);
            var result = await handler.Handle(cmd, ct);

            return result.Match(
                ok => Results.Ok(new GoogleSignIn.Response(ok.AccountId, ok.AccessToken, ok.RefreshToken, ok.ExpiresAtUtc)),
                CustomResults.Problem
            );
        })
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    public sealed record Command(IdentityProvider Provider, string IdToken, Dictionary<string, string>? Meta);

    public sealed class Handler(
        IProviderTokenVerifier verifier,
        IAccountRepository accounts,
        IIdentityRepository identities,
        IIdentityLoginAuditRepository audits,
        ITokenService tokens)
    {
        public async Task<Result<DeviceSignInFeature.AuthResult>> Handle(Command command, CancellationToken ct)
        {
            var subject = await verifier.VerifyAndGetSubjectAsync(command.Provider, command.IdToken, ct);
            if (subject is null)
                return Result.Failure<DeviceSignInFeature.AuthResult>(Error.Unauthorized("Auth.InvalidToken", "Provider token is invalid."));

            var now = DateTime.UtcNow;
            var found = await identities.GetByProviderAndSubject(command.Provider, subject, ct);
            if (found is not null)
            {
                found.TouchSeen(now, command.Meta);
                var acc = await accounts.GetById(found.AccountId, ct);
                acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

                await identities.SaveChanges(ct);
                await accounts.SaveChanges(ct);

                var issued = await tokens.IssueAsync(found.AccountId, ct);
                return new DeviceSignInFeature.AuthResult(found.AccountId, issued.access, issued.refresh, issued.expiresAtUtc);
            }

            var account = Account.Create(Guid.NewGuid(), now);
            await accounts.Add(account, ct);

            var identity = IdentityEntity.Create(Guid.NewGuid(), account.AccountId, command.Provider, subject, now, true);
            identity.TouchSeen(now, command.Meta);

            await identities.Add(identity, ct);
            await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), ct);

            await identities.SaveChanges(ct);
            await accounts.SaveChanges(ct);

            var pair = await tokens.IssueAsync(account.AccountId, ct);
            return new DeviceSignInFeature.AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
        }
    }
}
