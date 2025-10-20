using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record GoogleSignInCommand(IdentityProvider Provider, string IdToken, Dictionary<string, string>? Meta) : ICommand<AuthResult>;

public sealed class GoogleSignInCommandHandler(
    [FromKeyedServices("google")] IProviderTokenVerifier googleVerifier,
    IAccountRepository accounts,
    IIdentityRepository identities,
    IIdentityLoginAuditRepository audits,
    IDateTimeProvider clock,
    ITokenService tokens,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<GoogleSignInCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(GoogleSignInCommand command, CancellationToken ct)
    {
        // Only Google is supported here; if you plan to re-use for others, inject a factory
        var verify = await googleVerifier.VerifyAsync(command.IdToken, new ProviderVerifyHints { /* Nonce/HostedDomain if needed */ }, ct);
        if (verify.IsFailure)
            return Result.Failure<AuthResult>(verify.Error);

        var subject = verify.Value;
        var now = clock.UtcNow;

        // Try existing identity
        var found = await identities.GetByProviderAndSubject(command.Provider, subject, ct);
        if (found is not null)
        {
            found.TouchSeen(now, command.Meta);

            var acc = await accounts.GetById(found.AccountId, ct);
            acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

            // audit every login
            await audits.Add(IdentityLoginAudit.FromMeta(found.IdentityId, found.AccountId, now, command.Meta), ct);

            await uow.SaveChangesAsync(ct);

            var (access, refresh, expires) = await tokens.IssueAsync(found.AccountId, ct);
            return new AuthResult(found.AccountId, access, refresh, expires);
        }

        // New account + identity
        var account = Account.CreateNew(now);
        await accounts.Add(account, ct);

        var identity = IdentityEntity.Create(
            Guid.NewGuid(),
            account.AccountId,
            command.Provider,
            subject,
            now,
            loginAllowed: true);
        identity.TouchSeen(now, command.Meta);

        await identities.Add(identity, ct);

        await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), ct);

        await uow.SaveChangesAsync(ct);

        var pair = await tokens.IssueAsync(account.AccountId, ct);
        return new AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
    }
}