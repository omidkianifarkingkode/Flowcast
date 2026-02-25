
using Identity.Application.Repositories;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record LinkGoogleAccountCommand(
    Guid AccountId,
    string GoogleId,
    string? DisplayName,
    Dictionary<string, string>? Meta
    ) : ICommand;
public class LinkGoogleAccountCommandHandler(
    IAccountRepository accounts,
    IIdentityRepository identities,
    IDateTimeProvider clock,
    [FromKeyedServices("Identity")] IUnitOfWork uow
    ) : ICommandHandler<LinkGoogleAccountCommand>
{
    public async Task<Result> Handle(LinkGoogleAccountCommand command, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var googleId = command.GoogleId?.Trim();

        // Step 1: Validate input constraints
        if(string.IsNullOrWhiteSpace(googleId))
            return Result.Failure(DomainErrors.InvalidGoogleUserId);

        // Step 2: Ensure this Google account isn't already linked to a different identity
        var existingIdentity = await identities.GetByProviderAndSubject(IdentityProvider.Google, googleId, ct);
        if(existingIdentity is not null)
            return Result.Failure(Error.Conflict(
                "Identity.GoogleAlreadyLinked",
                "This Google account is already linked to another user."));

        // Step 3: Load the account aggregate
        var account = await accounts.GetById(command.AccountId, ct);
        if(account is null) return Result.Failure(Error.DefaultNotFound);

        // Step 4: Hydrate the aggregate with its current identities before processing domain logic
        var currentIdentities = await identities.GetByAccountId(command.AccountId, ct);
        account.AttachIdentities(currentIdentities);

        // Step 5: Execute domain logic to link the provider and disable legacy identities (Guest/Device)
        var linkResult = account.LinkProvider(IdentityProvider.Google, googleId, now);
        if(linkResult.IsFailure) return Result.Failure(linkResult.Error);

        // Step 6: Sync display name if provided
        if(!string.IsNullOrWhiteSpace(command.DisplayName))
            account.SetDisplayName(command.DisplayName);

        // Step 7: Add the new identity and enforce persistence rules
        await identities.Add(linkResult.Value, ct);

        // Step 8: Explicitly disable device logins in the repository layer
        await identities.DisableDeviceForAccount(command.AccountId, ct);

        // Step 9: Commit transaction
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
