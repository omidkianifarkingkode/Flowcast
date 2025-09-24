using Matchmaking.Domain;
using SharedKernel;
using SharedKernel.Primitives;

namespace MatchMaking.Application.Shared;

public interface ITicketRepository
{
    Task<Result<Ticket>> GetById(TicketId id, CancellationToken ct = default);
    Task<Result<Ticket>> GetOpenByPlayer(PlayerId playerId, string mode, CancellationToken ct = default); // Searching|PendingReady
    Task<Result<IReadOnlyList<Ticket>>> GetSearchingByMode(string mode, CancellationToken ct = default);   // FIFO snapshot
    Task Save(Ticket ticket, CancellationToken ct = default);
    Task Delete(TicketId id, CancellationToken ct = default);
}
