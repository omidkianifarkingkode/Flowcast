using Identity.Domain.Entities;

namespace Identity.Application.Repositories;

public interface IIdentityLoginAuditRepository
{
    Task Add(IdentityLoginAudit audit, CancellationToken ct);
}
