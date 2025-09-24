using Matchmaking.Domain;
using MatchMaking.Application.Shared;
using Shared.Application.Messaging;
using SharedKernel;

namespace MatchMaking.Application.Queries;

public record GetTicketQuery(TicketId TicketId) : IQuery<Ticket>;
public sealed class GetTicketHandler(ITicketRepository repo) : IQueryHandler<GetTicketQuery, Ticket>
{
    public async Task<Result<Ticket>> Handle(GetTicketQuery query, CancellationToken ct)
        => await repo.GetById(query.TicketId, ct);
}
