using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record GuestSignInCommand(Dictionary<string, string>? Meta) : ICommand<AuthResult>;

public sealed class GuestSignInCommandHandler(
    IAccountRepository accounts,
    IIdentityRepository identities,
    IIdentityLoginAuditRepository audits,
    IDateTimeProvider clock,
    ITokenService tokens,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<GuestSignInCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(GuestSignInCommand command, CancellationToken ct)
    {
        var now = clock.UtcNow;
        // Prefer a stable client-provided guestToken; fall back to a new synthetic subject
        var subject = command.Meta?.GetValueOrDefault("guestToken")?.Trim()
            ?? Guid.NewGuid().ToString("N");

        var existing = await identities.GetByProviderAndSubject(IdentityProvider.None, subject, ct);
        // Existing guest identity: reuse account and keep current display name
        if (existing is not null)
        {
            if (!existing.LoginAllowed)
                return Result.Failure<AuthResult>(DomainErrors.GuestLoginDisabled);

            existing.TouchSeen(now, command.Meta);

            var acc = await accounts.GetById(existing.AccountId, ct);
            acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

            await audits.Add(IdentityLoginAudit.FromMeta(existing.IdentityId, existing.AccountId, now, command.Meta), ct);

            await uow.SaveChangesAsync(ct);

            var (access, refresh, expiresAtUtc) = await tokens.IssueAsync(existing.AccountId, ct);
            return new AuthResult(existing.AccountId, access, refresh, expiresAtUtc);
        }

        // No existing guest identity: create a new guest account with a generated display name
        var account = Account.CreateNewForGuest(now, $"Guest_{Random.Shared.Next(100_000, 99_999_999)}");
        await accounts.Add(account, ct);

        var addResult = account.AddOrTouchGuestIdentity(subject, now, command.Meta);
        if (addResult.IsFailure) return Result.Failure<AuthResult>(addResult.Error);

        var identity = addResult.Value;
        await identities.Add(identity, ct);

        await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), ct);

        await uow.SaveChangesAsync(ct);

        var pair = await tokens.IssueAsync(account.AccountId, ct);
        return new AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
    }
}
