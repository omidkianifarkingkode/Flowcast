using Identity.API.Persistence.Entities;

namespace Identity.API.Services.Repositories;

public interface IKeyRepository
{
    Task<SigningKey?> GetActiveAsync(CancellationToken ct);
    Task<IReadOnlyList<SigningKey>> GetValidForValidationAsync(DateTime nowUtc, CancellationToken ct);
    Task<SigningKey?> FindByKidAsync(string kid, CancellationToken ct);
    Task AddAsync(SigningKey key, CancellationToken ct);
    Task DeactivateAllAsync(CancellationToken ct);
}
