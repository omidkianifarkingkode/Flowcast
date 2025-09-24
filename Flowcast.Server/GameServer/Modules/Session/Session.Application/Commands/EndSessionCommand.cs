using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Session.Application.Commands;

public record EndSessionCommand(SessionId SessionId, string? Reason = null) : ICommand<SessionEntity>;

public sealed class EndSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider clock)
    : ICommandHandler<EndSessionCommand, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(EndSessionCommand command, CancellationToken cancellationToken)
    {
        var getResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (getResult.IsFailure)
            return Result.Failure<SessionEntity>(getResult.Error ?? SessionErrors.SessionNotFound);

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


