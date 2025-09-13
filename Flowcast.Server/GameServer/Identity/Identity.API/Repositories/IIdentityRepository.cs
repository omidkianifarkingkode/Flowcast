using Identity.API.Entities;
using Identity.API.Shared;

namespace Identity.API.Repositories;

public interface IIdentityRepository
{
    Task<IdentityEntity?> GetByProviderAndSubject(IdentityProvider provider, string subject, CancellationToken ct);
    Task<IReadOnlyList<IdentityEntity>> GetByAccountId(Guid accountId, CancellationToken ct);

    Task<bool> ExistsNonDeviceForAccount(Guid accountId, CancellationToken ct);
    Task Add(IdentityEntity identity, CancellationToken ct);
    Task SaveChanges(CancellationToken ct);

    /// <summary>Bulk disable device identities for an account (post-link effect).</summary>
    Task DisableDeviceForAccount(Guid accountId, CancellationToken ct);
}
