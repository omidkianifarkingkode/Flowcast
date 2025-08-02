using Application.Sessions.Shared;
using Domain.Entities;
using Domain.Players;
using Domain.Sessions;
using Domain.ValueObjects;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Commands;

public record CreateSessionCommand(List<PlayerDto> Players, string Mode, MatchSettings? GameSettings) 
    : IRequest<Result<SessionId>>;


public sealed class CreateSessionHandler(ISessionRepository sessionRepository)
    : IRequestHandler<CreateSessionCommand, Result<SessionId>>
{
    public Task<Result<SessionId>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var sessionId = SessionId.NewId();

        var session = new Session(sessionId, request.Mode, request.GameSettings);

        foreach (var player in request.Players)
        {
            session.AddPlayer(new Player(player.Id, player.DisplayName));
        }

        // No auto-start here — wait for players to signal readiness

        sessionRepository.Save(session);

        return Task.FromResult(Result.Success(sessionId));
    }
}


