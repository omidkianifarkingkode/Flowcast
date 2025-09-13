using Identity.API.Entities;

namespace Identity.API.Repositories;

public interface IIdentityLoginAuditRepository
{
    Task Add(IdentityLoginAudit audit, CancellationToken ct);
}
