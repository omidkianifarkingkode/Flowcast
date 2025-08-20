using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record EndSessionCommand(SessionId SessionId, string? Reason = null) : ICommand<Session>;

public sealed class EndSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider clock) 
    : ICommandHandler<EndSessionCommand, Session>
{
    public async Task<Result<Session>> Handle(EndSessionCommand command, CancellationToken cancellationToken)
    {
        var getResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (getResult.IsFailure)
            return Result.Failure<Session>(getResult.Error ?? SessionErrors.SessionNotFound);

        var session = getResult.Value;

        var parsed = Enum.TryParse<SessionCloseReason>(
            command.Reason ?? nameof(SessionCloseReason.Completed),
            ignoreCase: true, out var reason)
                 ? reason
                 : SessionCloseReason.Completed;
        
        session.End(clock.UtcNow, parsed);

        await sessionRepository.Save(session, cancellationToken);

        return session;
    }
}


