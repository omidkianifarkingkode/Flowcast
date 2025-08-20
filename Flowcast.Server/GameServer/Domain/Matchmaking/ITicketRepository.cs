using Domain.Sessions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Matchmaking;

public interface ITicketRepository
{
    Task<Result<Ticket>> GetById(TicketId id, CancellationToken ct = default);
    Task<Result<Ticket?>> GetOpenByPlayer(PlayerId playerId, string mode, CancellationToken ct = default); // Searching or PendingReady
    Task<Result<IReadOnlyList<Ticket>>> GetSearchingByMode(string mode, CancellationToken ct = default);
    Task Save(Ticket ticket, CancellationToken ct = default);
}

public interface IMatchRepository
{
    Task<Result<Match>> GetById(MatchId id, CancellationToken ct = default);
    Task Save(Match match, CancellationToken ct = default);
}