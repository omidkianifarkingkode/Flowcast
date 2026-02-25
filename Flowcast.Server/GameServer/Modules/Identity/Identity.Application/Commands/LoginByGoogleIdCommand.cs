using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record LoginByGoogleIdCommand(string GoogleUserId, Dictionary<string, string>? Meta) : ICommand<AuthResult>;

public sealed class LoginByGoogleIdCommandHandler(
    IAccountRepository accounts,
    IIdentityRepository identities,
    IIdentityLoginAuditRepository audits,
    IDateTimeProvider clock,
    ITokenService tokens,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<LoginByGoogleIdCommand, AuthResult>
{
    private const int MaxGoogleUserIdLength = 128;

    public async Task<Result<AuthResult>> Handle(LoginByGoogleIdCommand command, CancellationToken ct)
    {
        // Step 1: Validate Google user id (trim, non-empty, max length)
        var subject = command.GoogleUserId?.Trim();
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > MaxGoogleUserIdLength)
            return Result.Failure<AuthResult>(DomainErrors.InvalidGoogleUserId);

        var now = clock.UtcNow;
        // Step 2: Look up existing identity by Google provider and subject
        var found = await identities.GetByProviderAndSubject(IdentityProvider.Google, subject, ct);
        if (found is not null)
        {
            // Step 3a (existing user): Update identity last-seen and meta
            found.TouchSeen(now, command.Meta);

            // Step 4a: Load account and update last login (and optional region from meta)
            var acc = await accounts.GetById(found.AccountId, ct);
            acc?.TouchLastLogin(now, command.Meta?.TryGetValue("region", out var r) == true ? r : null);

            // Step 5a: Record login audit and persist
            await audits.Add(IdentityLoginAudit.FromMeta(found.IdentityId, found.AccountId, now, command.Meta), ct);
            await uow.SaveChangesAsync(ct);

            // Step 6a: Issue tokens and return auth result for existing user
            var (access, refresh, expires) = await tokens.IssueAsync(found.AccountId, ct);
            return new AuthResult(found.AccountId, access, refresh, expires);
        }

        // Step 3b (new user): Create account and add to repository
        var account = Account.CreateNew(now);
        await accounts.Add(account, ct);

        // Step 4b: Create Google identity linked to account and set last-seen
        var identity = IdentityEntity.Create(
            Guid.NewGuid(),
            account.AccountId,
            IdentityProvider.Google,
            subject,
            now,
            loginAllowed: true);
        identity.TouchSeen(now, command.Meta);

        // Step 5b: Persist identity, add login audit, save
        await identities.Add(identity, ct);
        await audits.Add(IdentityLoginAudit.FromMeta(identity.IdentityId, account.AccountId, now, command.Meta), ct);
        await uow.SaveChangesAsync(ct);

        // Step 6b: Issue tokens and return auth result for new user
        var pair = await tokens.IssueAsync(account.AccountId, ct);
        return new AuthResult(account.AccountId, pair.access, pair.refresh, pair.expiresAtUtc);
    }
}
