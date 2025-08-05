using Application.Abstractions.Messaging;
using Application.Services;
using Domain.Sessions;
using Domain.Sessions.Entities;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record LeaveSessionCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<LeaveSessionResult>;

public record LeaveSessionResult(Session Session, Player Player, bool WasLastPlayer);

public sealed class LeaveSessionHandler(ISessionRepository sessionRepository, ConnectedPlayersRegistry playerRegistry) 
    : ICommandHandler<LeaveSessionCommand, LeaveSessionResult>
{
    public async Task<Result<LeaveSessionResult>> Handle(LeaveSessionCommand command, CancellationToken cancellationToken)
    {
        if (!playerRegistry.TryGet(command.PlayerId.Value, out var registerdPlayer))
            return Result.Failure<LeaveSessionResult>(SessionErrors.PlayerNotFound);

        var sessionResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<LeaveSessionResult>(sessionResult.Error);

        var session = sessionResult.Value;

        var playerToJoin = new Player(command.PlayerId, registerdPlayer.DisplayName);

        var removeResult = session.RemovePlayer(playerToJoin);
        if (removeResult.IsFailure)
            return Result.Failure<LeaveSessionResult>(removeResult.Error);

        await sessionRepository.Save(session, cancellationToken);

        var wasLastPlayer = session.Players.Count == 0;

        return Result.Success(new LeaveSessionResult(session, playerToJoin, wasLastPlayer));
    }
}
