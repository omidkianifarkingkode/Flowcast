using Identity.Domain.Entities;

namespace Identity.Application.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetById(Guid accountId, CancellationToken ct);
    Task Add(Account account, CancellationToken ct);
}
