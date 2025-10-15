using PlayerProgressStore.Infrastructure.Persistence;
using Shared.Infrastructure.Database;

namespace PlayerProgressStore.Infrastructure.Services;

public class PlayerProgressUnitOfWork(ApplicationDbContext context) : UnitOfWork<ApplicationDbContext>(context)
{
}
