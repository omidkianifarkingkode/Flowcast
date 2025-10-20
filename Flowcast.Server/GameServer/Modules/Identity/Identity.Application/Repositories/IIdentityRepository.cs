using Identity.Domain.Entities;
using Identity.Domain.Shared;

namespace Identity.Application.Repositories;

public interface IIdentityRepository
{
    Task<IdentityEntity?> GetByProviderAndSubject(IdentityProvider provider, string subject, CancellationToken ct);
    Task<IReadOnlyList<IdentityEntity>> GetByAccountId(Guid accountId, CancellationToken ct);
    Task<bool> ExistsNonDeviceForAccount(Guid accountId, CancellationToken ct);
    Task Add(IdentityEntity identity, CancellationToken ct);
    Task DisableDeviceForAccount(Guid accountId, CancellationToken ct);
}
