using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record DeviceSignInCommand(string DeviceId, Dictionary<string, string>? Meta) : ICommand<AuthResult>;

public sealed class DeviceSignInCommandHandler(
    IAccountRepository accounts,
    IIdentityRepository identities,
    IIdentityLoginAuditRepository audits,
    IDateTimeProvider clock,
    ITokenService tokens,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<DeviceSignInCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(DeviceSignInCommand command, CancellationToken ct)
    {
        var now = clock.UtcNow;

        // try find existing device identity
        var existing = await identities.GetByProviderAndSubject(IdentityProvider.Device, command.DeviceId.Trim(), ct);
        if (existing is not null)
        {
            if (!existing.LoginAllowed)
                return Result.Failure<AuthResult>(DomainErrors.DeviceLoginDisabled);

            existing.TouchSeen(now, command.Meta);

            var acc = await accounts.GetById(existing.AccountId, ct);
            acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

            // audit every login
            await audits.Add(IdentityLoginAudit.FromMeta(existing.IdentityId, existing.AccountId, now, command.Meta), ct);

            await uow.SaveChangesAsync(ct);

            var (access, refresh, expiresAtUtc) = await tokens.IssueAsync(existing.AccountId, ct);
            return new AuthResult(existing.AccountId, access, refresh, expiresAtUtc);
        }

        // create new account + device identity
        var account = Account.CreateNew(now);
        await accounts.Add(account, ct);

        var addResult = account.AddOrTouchDeviceIdentity(command.DeviceId.Trim(), now, command.Meta);
        if (addResult.IsFailure) return Result.Failure<AuthResult>(addResult.Error);

        var identity = addResult.Value;
        await identities.Add(identity, ct);

        await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), ct);

        await uow.SaveChangesAsync(ct);

        var pair = await tokens.IssueAsync(account.AccountId, ct);
        return new AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
    }
}
