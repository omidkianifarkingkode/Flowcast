using Identity.API.Persistence.Entities;

namespace Identity.API.Services.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetById(Guid accountId, CancellationToken ct);
    Task Add(Account account, CancellationToken ct);
}
