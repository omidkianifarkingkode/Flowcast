using Microsoft.EntityFrameworkCore;
using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Domain;
using SharedKernel;

namespace PlayerProgressStore.Infrastructure.Persistence
{

    public class PlayerNamespaceRepository(ApplicationDbContext context) : IPlayerNamespaceRepository
    {
        public async Task<Result<IReadOnlyList<PlayerNamespace>>> LoadAsync(
            string playerId,
            IReadOnlyCollection<string>? namespaces,
            CancellationToken ct)
        {
            var query = context.PlayerNamespaces.AsQueryable();

            if (namespaces != null && namespaces.Any())
            {
                query = query.Where(ns => namespaces.Contains(ns.Namespace));
            }

            var namespacesList = await query
                .Where(ns => ns.PlayerId == playerId)
                .ToListAsync(ct);

            return Result.Success((IReadOnlyList<PlayerNamespace>)namespacesList);
        }

        public async Task<Result<IReadOnlyList<PlayerNamespace>>> UpsertAtomicAsync(
            string playerId,
            IReadOnlyList<PlayerNamespace> updated,
            CancellationToken ct)
        {
            foreach (var namespaceItem in updated)
            {
                var existingNamespace = await context.PlayerNamespaces
                    .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Namespace == namespaceItem.Namespace, ct);

                if (existingNamespace != null)
                {
                    // Update existing
                    context.Entry(existingNamespace).CurrentValues.SetValues(namespaceItem);
                }
                else
                {
                    // Insert new
                    await context.PlayerNamespaces.AddAsync(namespaceItem, ct);
                }
            }

            return Result.Success(updated);
        }
    }
}
