using Application.Sessions.Shared;
using Domain.Sessions;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionQuery(SessionId SessionId) : IRequest<Result<SessionDto>>;

public sealed class GetSessionHandler(ISessionRepository repo)
    : IRequestHandler<GetSessionQuery, Result<SessionDto>>
{
    public Task<Result<SessionDto>> Handle(GetSessionQuery request, CancellationToken ct)
    {
        var result = repo.GetById(request.SessionId);

        if (result.IsFailure)
            return Task.FromResult(Result.Failure<SessionDto>(result.Error));

        var session = result.Value;

        var dto = new SessionDto(
            session.Id.Value,
            session.Mode,
            session.Status,
            session.CreatedAtUtc,
            session.Players.Select(p => new PlayerDto(p.PlayerId, p.DisplayName, p.Status)).ToList()
        );

        return Task.FromResult(Result.Success(dto));
    }
}
