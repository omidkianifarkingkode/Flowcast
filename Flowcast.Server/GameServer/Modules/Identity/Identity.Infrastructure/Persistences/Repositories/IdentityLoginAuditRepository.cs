using Identity.Application.Repositories;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Persistences.Repositories;

public sealed class IdentityLoginAuditRepository(ApplicationDbContext db) : IIdentityLoginAuditRepository
{
    public Task Add(IdentityLoginAudit audit, CancellationToken ct)
        => db.IdentityLoginAudits.AddAsync(audit, ct).AsTask();
}