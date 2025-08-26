using Application.Abstractions.Messaging;
using Domain.Matchmaking;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MatchMakings.Queries;

public record GetTicketQuery(TicketId TicketId) : IQuery<Ticket>;
public sealed class GetTicketHandler(ITicketRepository repo) : IQueryHandler<GetTicketQuery, Ticket>
{
    public async Task<Result<Ticket>> Handle(GetTicketQuery query, CancellationToken ct)
        => await repo.GetById(query.TicketId, ct);
}
