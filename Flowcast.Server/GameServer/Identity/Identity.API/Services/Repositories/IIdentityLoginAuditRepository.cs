using Identity.API.Persistence.Entities;

namespace Identity.API.Services.Repositories;

public interface IIdentityLoginAuditRepository
{
    Task Add(IdentityLoginAudit audit, CancellationToken ct);
}
