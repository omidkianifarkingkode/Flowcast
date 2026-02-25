using Identity.Application.Repositories;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record UnlinkProviderCommand(
    Guid AccountId,
    IdentityProvider Provider
    ) : ICommand;
public sealed class UnlinkProviderCommandHandler(
    IIdentityRepository identities,
    [FromKeyedServices("identity")] IUnitOfWork uow
    ) : ICommandHandler<UnlinkProviderCommand>
{
    public async Task<Result> Handle(UnlinkProviderCommand command, CancellationToken ct)
    {
        // Step 1: Fetch all identities associated with the account
        var accIdentities = await identities.GetByAccountId(command.AccountId, ct);

        // Step 2: Find the target identity for the specified provider
        var target = accIdentities.FirstOrDefault(
            x => x.Provider == command.Provider);

        if(target is null)
            return Result.Failure(DomainErrors.IdentityNotFound);

        // Step 3: Domain Guard - Prevent unlinking if it's the last remaining login method
        if(accIdentities.Count(i => i.LoginAllowed) <= 1)
            return Result.Failure(Error.Conflict(
                "Identity.LastProvider",
                "You cannot unlink the last available login method."));

        // Step 4: Perform the unlink by disabling the identity's login capability
        target.DisableLogin();

        // Step 5: Persist changes via Unit of Work
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}