using Identity.API.Persistence.Entities;
using Identity.API.Services.Repositories;
using Identity.Contracts.V1.Shared;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Persistence.Repositories;

public sealed class IdentityRepository(IdentityDbContext db) : IIdentityRepository
{
    public Task<IdentityEntity?> GetByProviderAndSubject(IdentityProvider provider, string subject, CancellationToken ct)
        => db.Identities.FirstOrDefaultAsync(i => i.Provider == provider && i.Subject == subject.Trim(), ct);

    public async Task<IReadOnlyList<IdentityEntity>> GetByAccountId(Guid accountId, CancellationToken ct)
        => await db.Identities.Where(i => i.AccountId == accountId)
                               .OrderBy(i => i.CreatedAtUtc)
                               .ToListAsync(ct);

    public Task<bool> ExistsNonDeviceForAccount(Guid accountId, CancellationToken ct)
        => db.Identities.AnyAsync(i => i.AccountId == accountId && i.Provider != IdentityProvider.Device, ct);

    public Task Add(IdentityEntity identity, CancellationToken ct)
        => db.Identities.AddAsync(identity, ct).AsTask();

    public async Task DisableDeviceForAccount(Guid accountId, CancellationToken ct)
    {
        try
        {
            await db.Identities
                     .Where(i => i.AccountId == accountId && i.Provider == IdentityProvider.Device && i.LoginAllowed)
                     .ExecuteUpdateAsync(s => s.SetProperty(i => i.LoginAllowed, false), ct);
        }
        catch (NotSupportedException)
        {
            var devices = await db.Identities
                                   .Where(i => i.AccountId == accountId && i.Provider == IdentityProvider.Device && i.LoginAllowed)
                                   .ToListAsync(ct);
            foreach (var d in devices) d.DisableLogin();
        }
    }
}
