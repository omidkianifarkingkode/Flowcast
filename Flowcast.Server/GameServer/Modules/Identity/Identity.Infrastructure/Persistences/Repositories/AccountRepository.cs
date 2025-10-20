using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistences.Repositories;

public sealed class AccountRepository(ApplicationDbContext db) : IAccountRepository
{
    public Task<Account?> GetById(Guid accountId, CancellationToken ct)
        => db.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId, ct);

    public Task Add(Account account, CancellationToken ct)
        => db.Accounts.AddAsync(account, ct).AsTask();
}