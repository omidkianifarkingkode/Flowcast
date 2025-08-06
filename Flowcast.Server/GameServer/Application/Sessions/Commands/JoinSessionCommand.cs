using Application.Abstractions.Messaging;
using Application.Services;
using Domain.Sessions;
using Domain.Sessions.Entities;
using Domain.Sessions.Errors;
using Domain.Sessions.Services;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record JoinSessionCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<JoinSessionResult>;

public record JoinSessionResult(Session Session, Player Player);

public sealed class JoinSessionHandler(ISessionRepository sessionRepository, ConnectedPlayersRegistry playerRegistry) 
    : ICommandHandler<JoinSessionCommand, JoinSessionResult>
{
    public async Task<Result<JoinSessionResult>> Handle(JoinSessionCommand command, CancellationToken cancellationToken)
    {
        if (!playerRegistry.TryGet(command.PlayerId.Value, out var registerdPlayer))
            return Result.Failure<JoinSessionResult>(SessionErrors.PlayerNotFound);

        var sessionResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<JoinSessionResult>(sessionResult.Error);

        var session = sessionResult.Value;

        var playerToJoin = new Player(command.PlayerId, registerdPlayer.DisplayName);
        var joinResult = session.JoinPlayer(playerToJoin);
        if(joinResult.IsFailure)
            return Result.Failure<JoinSessionResult>(joinResult.Error);

        await sessionRepository.Save(session, cancellationToken);

        return Result.Success(new JoinSessionResult(session, playerToJoin));
    }
}



