using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Sessions;
using Domain.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record CreateSessionCommand(List<CreateSessionCommand.PlayerInput> Players, string Mode, MatchSettings? GameSettings)
    : ICommand<SessionId>
{
    public record PlayerInput(long Id, string DisplayName);
}

public sealed class CreateSessionHandler(ISessionRepository sessionRepository)
    : ICommandHandler<CreateSessionCommand, SessionId>
{
    public Task<Result<SessionId>> Handle(CreateSessionCommand request, CancellationToken ct)
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


