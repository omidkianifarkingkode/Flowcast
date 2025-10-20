using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistences.Repositories;

public sealed class KeyRepository(ApplicationDbContext db) : IKeyRepository
{
    public Task<SigningKey?> GetActiveAsync(CancellationToken ct) =>
        db.SigningKeys.AsNoTracking().FirstOrDefaultAsync(k => k.IsActive, ct);

    public async Task<IReadOnlyList<SigningKey>> GetValidForValidationAsync(DateTime nowUtc, CancellationToken ct)
    {
        return await db.SigningKeys.AsNoTracking()
            .Where(k => k.NotBeforeUtc <= nowUtc && (k.ExpiresAtUtc == null || k.ExpiresAtUtc > nowUtc))
            .OrderByDescending(k => k.IsActive).ThenByDescending(k => k.NotBeforeUtc)
            .ToListAsync(ct);
    }

    public Task<SigningKey?> FindByKidAsync(string kid, CancellationToken ct) =>
        db.SigningKeys.FirstOrDefaultAsync(k => k.KeyId == kid, ct);

    public Task AddAsync(SigningKey key, CancellationToken ct) =>
        db.SigningKeys.AddAsync(key, ct).AsTask();

    public async Task DeactivateAllAsync(CancellationToken ct)
    {
        try
        {
            await db.SigningKeys.Where(k => k.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.IsActive, false), ct);
        }
        catch (NotSupportedException)
        {
            var active = await db.SigningKeys.Where(k => k.IsActive).ToListAsync(ct);
            foreach (var k in active) k.Deactivate();
        }
    }
}
