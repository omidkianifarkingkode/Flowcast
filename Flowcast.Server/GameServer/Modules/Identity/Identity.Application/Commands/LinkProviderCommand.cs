using Identity.Application.Repositories;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;
/// <summary>
/// This class for suggest a generic structure for Multi-Identity Architecture
/// </summary>
/// <param name="AccountId"></param>
/// <param name="Provider"></param>
/// <param name="SubjectId"></param>
/// <param name="DisplayName"></param>
/// <param name="Meta"></param>
public sealed record LinkProviderCommand(
    Guid AccountId,
    IdentityProvider Provider,
    string SubjectId,
    string? DisplayName,
    Dictionary<string, string>? Meta) : ICommand;

public sealed class LinkProviderCommandHandler(
    IAccountRepository accounts,
    IIdentityRepository identities,
    IDateTimeProvider clock,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<LinkProviderCommand>
{
    public async Task<Result> Handle(LinkProviderCommand command, CancellationToken ct)
    {
        var now = clock.UtcNow;

        // Step 1: Ensure this provider+subject is not already linked to another account
        var existing = await identities.GetByProviderAndSubject(command.Provider, command.SubjectId, ct);
        if(existing is not null)
            return Result.Failure(Error.Conflict("Identity.ProviderInUse", "This identity is already linked to another account."));

        // Step 2: Load the current account
        var account = await accounts.GetById(command.AccountId, ct);
        if(account is null) return Result.Failure(Error.DefaultNotFound);

        // Step 3: Attach current identities to enforce business rules (like disabling Guest)
        var currentIdentities = await identities.GetByAccountId(command.AccountId, ct);
        account.AttachIdentities(currentIdentities);

        // Step 4: Use the Domain Model to link (This handles Guest/Device disabling)
        var linkResult = account.LinkProvider(command.Provider, command.SubjectId, now);
        if(linkResult.IsFailure) return Result.Failure(linkResult.Error);

        if(!string.IsNullOrWhiteSpace(command.DisplayName))
            account.SetDisplayName(command.DisplayName);

        // Step 5: Persistence
        await identities.Add(linkResult.Value, ct);
        await identities.DisableDeviceForAccount(command.AccountId, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}