using Identity.API.Entities;
using Identity.API.Repositories;
using Identity.API.Services;
using Identity.API.Shared;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Identity.API.Endpoints;

public static class DeviceSignInFeature
{
    public static void Map(WebApplication app)
    {
        app.MapPost(DeviceSignIn.Route, async (
            DeviceSignIn.Request request,
            Handler handler,
            CancellationToken ct) =>
        {
            var cmd = new Command(request.DeviceId, request.Meta);
            var result = await handler.Handle(cmd, ct);

            return result.Match(
                ok => Results.Ok(new DeviceSignIn.Response(ok.AccountId, ok.AccessToken, ok.RefreshToken, ok.ExpiresAtUtc)),
                CustomResults.Problem
            );
        })
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    // -------- Command DTO --------
    public sealed record Command(string DeviceId, Dictionary<string, string>? Meta);

    // -------- Handler --------
    public sealed class Handler(
        IAccountRepository accounts,
        IIdentityRepository identities,
        IIdentityLoginAuditRepository audits,
        IDateTimeProvider clock,
        ITokenService tokens)
    {
        public async Task<Result<AuthResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var now = clock.UtcNow;

            var found = await identities.GetByProviderAndSubject(IdentityProvider.Device, command.DeviceId, cancellationToken);
            if (found is not null)
            {
                if (!found.LoginAllowed)
                    return Result.Failure<AuthResult>(DomainErrors.DeviceLoginDisabled);

                found.TouchSeen(now, command.Meta);
                var acc = await accounts.GetById(found.AccountId, cancellationToken);
                acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

                await identities.SaveChanges(cancellationToken);
                await accounts.SaveChanges(cancellationToken);

                var (access, refresh, expiresAtUtc) = await tokens.IssueAsync(found.AccountId, cancellationToken);
                return new AuthResult(found.AccountId, access, refresh, expiresAtUtc);
            }

            var account = Account.CreateNew(now);
            await accounts.Add(account, cancellationToken);

            var addResult = account.AddOrTouchDeviceIdentity(command.DeviceId, now, command.Meta);
            if (addResult.IsFailure) return Result.Failure<AuthResult>(addResult.Error);

            var identity = addResult.Value;
            await identities.Add(identity, cancellationToken);
            await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), cancellationToken);

            await identities.SaveChanges(cancellationToken);
            await accounts.SaveChanges(cancellationToken);

            var pair = await tokens.IssueAsync(account.AccountId, cancellationToken);
            return new AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
        }
    }

    // -------- Result DTO used by handlers --------
    public sealed record AuthResult(Guid AccountId, string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
}
