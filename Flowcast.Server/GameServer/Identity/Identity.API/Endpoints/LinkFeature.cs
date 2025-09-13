// /Identity/V1/LinkFeature.cs
using Identity.API.Extensions;
using Identity.API.Repositories;
using Identity.API.Services;
using Identity.API.Shared;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Identity.API.Endpoints;

public static class LinkFeature
{
    public static void Map(WebApplication app)
    {
        app.MapPost(Link.Route, async (
            HttpContext http,
            Link.Request request,
            Handler handler,
            CancellationToken ct) =>
        {
            var accountId = http.GetAccountId();
            var provider = Enum.Parse<IdentityProvider>(request.Provider);
            var cmd = new Command(accountId, provider, request.IdToken, request.DisplayName, request.Meta);
            var result = await handler.Handle(cmd, ct);

            return result.Match(
                _ => Results.Ok(new Link.Response(accountId, Linked: true)),
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("Identity")
        .MapToApiVersion(1.0);
    }

    public sealed record Command(Guid AccountId, IdentityProvider Provider, string IdToken, string? DisplayName, Dictionary<string, string>? Meta);

    public sealed class Handler(
        IProviderTokenVerifier verifier,
        IAccountRepository accounts,
        IIdentityRepository identities)
    {
        public async Task<Result<bool>> Handle(Command command, CancellationToken ct)
        {
            if (command.Provider == IdentityProvider.Device)
                return Result.Failure<bool>(DomainErrors.InvalidProvider);

            var subject = await verifier.VerifyAndGetSubjectAsync(command.Provider, command.IdToken, ct);
            if (subject is null)
                return Result.Failure<bool>(Error.Unauthorized("Auth.InvalidToken", "Provider token is invalid."));

            // Ensure not linked elsewhere
            var existing = await identities.GetByProviderAndSubject(command.Provider, subject, ct);
            if (existing is not null && existing.AccountId != command.AccountId)
                return Result.Failure<bool>(Error.Conflict("Identity.ProviderInUse", "Provider subject already linked to another account."));

            var acc = await accounts.GetById(command.AccountId, ct);
            if (acc is null) return Result.Failure<bool>(Error.DefaultNotFound);

            var accIds = await identities.GetByAccountId(command.AccountId, ct);
            acc.AttachIdentities(accIds);

            var linkRes = acc.LinkProvider(command.Provider, subject, DateTime.UtcNow);
            if (linkRes.IsFailure) return Result.Failure<bool>(linkRes.Error);

            if (!string.IsNullOrWhiteSpace(command.DisplayName))
                acc.SetDisplayName(command.DisplayName!);

            await identities.Add(linkRes.Value, ct);
            await identities.DisableDeviceForAccount(command.AccountId, ct);

            await identities.SaveChanges(ct);
            await accounts.SaveChanges(ct);
            return true;
        }
    }
}
