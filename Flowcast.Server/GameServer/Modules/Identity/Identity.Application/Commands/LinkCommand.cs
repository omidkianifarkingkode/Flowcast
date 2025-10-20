using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Application.Commands;

public sealed record LinkCommand(
    Guid AccountId,
    IdentityProvider Provider,
    string IdToken,
    string? DisplayName,
    Dictionary<string, string>? Meta) : ICommand;

public sealed class LinkCommandHandler(
    IServiceProvider services,
    IAccountRepository accounts,
    IIdentityRepository identities,
    IDateTimeProvider clock,
    [FromKeyedServices("identity")] IUnitOfWork uow) : ICommandHandler<LinkCommand>
{
    public async Task<Result> Handle(LinkCommand command, CancellationToken ct)
    {
        if (command.Provider == IdentityProvider.Device)
            return Result.Failure(DomainErrors.InvalidProvider);

        // resolve verifier by provider key (lowercase enum name)
        var key = command.Provider.ToString().ToLowerInvariant();

        IProviderTokenVerifier verifier;
        try
        {
            verifier = services.GetRequiredKeyedService<IProviderTokenVerifier>(key);
        }
        catch
        {
            return Result.Failure<bool>(Error.Failure(
                "Link.UnsupportedProvider",
                $"No verifier registered for provider '{command.Provider}'."));
        }

        // verify provider token
        var verify = await verifier.VerifyAsync(
            command.IdToken,
            new ProviderVerifyHints { /* Nonce/HostedDomain if needed */ },
            ct);

        if (verify.IsFailure)
            return Result.Failure(verify.Error);

        var subject = verify.Value;
        var now = clock.UtcNow;

        // Ensure not linked elsewhere
        var existing = await identities.GetByProviderAndSubject(command.Provider, subject, ct);
        if (existing is not null && existing.AccountId != command.AccountId)
            return Result.Failure(Error.Conflict(
                "Identity.ProviderInUse",
                "Provider subject already linked to another account."));

        // Load account
        var acc = await accounts.GetById(command.AccountId, ct);
        if (acc is null) return Result.Failure(Error.DefaultNotFound);

        // Attach current identities to aggregate before linking
        var accIds = await identities.GetByAccountId(command.AccountId, ct);
        acc.AttachIdentities(accIds);

        var linkRes = acc.LinkProvider(command.Provider, subject, now);
        if (linkRes.IsFailure) return Result.Failure(linkRes.Error);

        if (!string.IsNullOrWhiteSpace(command.DisplayName))
            acc.SetDisplayName(command.DisplayName);

        // Persist: add new linked identity + disable device identities
        await identities.Add(linkRes.Value, ct);
        await identities.DisableDeviceForAccount(command.AccountId, ct);

        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}