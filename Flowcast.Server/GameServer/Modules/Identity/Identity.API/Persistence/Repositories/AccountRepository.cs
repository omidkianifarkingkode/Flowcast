using Identity.API.Persistence.Entities;
using Identity.API.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Persistence.Repositories;

public sealed class AccountRepository(IdentityDbContext db) : IAccountRepository
{
    public Task<Account?> GetById(Guid accountId, CancellationToken ct)
        => db.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId, ct);

    public Task Add(Account account, CancellationToken ct)
        => db.Accounts.AddAsync(account, ct).AsTask();
}