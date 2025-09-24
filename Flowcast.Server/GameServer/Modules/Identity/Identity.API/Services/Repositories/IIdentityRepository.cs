using Identity.API.Persistence.Entities;
using Identity.API.Shared;
using Identity.Contracts.V1.Shared;

namespace Identity.API.Services.Repositories;

public interface IIdentityRepository
{
    Task<IdentityEntity?> GetByProviderAndSubject(IdentityProvider provider, string subject, CancellationToken ct);
    Task<IReadOnlyList<IdentityEntity>> GetByAccountId(Guid accountId, CancellationToken ct);
    Task<bool> ExistsNonDeviceForAccount(Guid accountId, CancellationToken ct);
    Task Add(IdentityEntity identity, CancellationToken ct);
    Task DisableDeviceForAccount(Guid accountId, CancellationToken ct);
}
