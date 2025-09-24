using Identity.API.Persistence.Entities;
using Identity.API.Services.Repositories;

namespace Identity.API.Persistence.Repositories;

public sealed class IdentityLoginAuditRepository(IdentityDbContext db) : IIdentityLoginAuditRepository
{
    public Task Add(IdentityLoginAudit audit, CancellationToken ct)
        => db.IdentityLoginAudits.AddAsync(audit, ct).AsTask();
}