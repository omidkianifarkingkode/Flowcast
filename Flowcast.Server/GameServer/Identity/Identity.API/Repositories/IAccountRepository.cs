using Identity.API.Entities;

namespace Identity.API.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetById(Guid accountId, CancellationToken ct);
    Task Add(Account account, CancellationToken ct);
    Task SaveChanges(CancellationToken ct);
}
