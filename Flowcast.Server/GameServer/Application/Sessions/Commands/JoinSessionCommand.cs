using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Realtime.Services;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record JoinSessionCommand(
    SessionId SessionId,
    PlayerId PlayerId,
    string JoinToken,
    string? BuildHash,
    string? DisplayName)
    : ICommand<JoinSessionResult>;

public record JoinSessionResult(Session Session, Participant Participant);

public sealed class JoinSessionHandler(
    ISessionRepository sessionRepository,
    IUserConnectionRegistry userConn,
    IJoinTokenValidator tokenValidator,
    IDateTimeProvider clock)
    : ICommandHandler<JoinSessionCommand, JoinSessionResult>
{
    public async Task<Result<JoinSessionResult>> Handle(JoinSessionCommand cmd, CancellationToken cancellationToken)
    {
        if (!tokenValidator.Validate(cmd.SessionId.Value, cmd.PlayerId.Value, cmd.JoinToken))
            return Result.Failure<JoinSessionResult>(Error.Unauthorized("session.invalid_join_token", "Join token invalid or expired."));

        if (!userConn.IsUserConnected(cmd.PlayerId.Value))
            return Result.Failure<JoinSessionResult>(Error.Conflict("session.player_not_connected", "Player socket not connected."));

        var sessionResult = await sessionRepository.GetById(cmd.SessionId, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<JoinSessionResult>(sessionResult.Error);

        var session = sessionResult.Value;

        var displayName = cmd.DisplayName ?? $"Player-{cmd.PlayerId.Value.ToString()[..8]}"; // snapshot; replace with directory later
        var participant = session.Participants.FirstOrDefault(x => x.Id == cmd.PlayerId)
            ?? new Participant(cmd.PlayerId, displayName);

        var joinResult = session.Participants.Any(x => x.Id == cmd.PlayerId)
            ? Result.Success()
            : session.JoinParticipant(participant, clock.UtcNow);

        if (joinResult.IsFailure)
            return Result.Failure<JoinSessionResult>(joinResult.Error);

        session.MarkParticipantConnected(cmd.PlayerId);
        // Optional: record BuildHash in an extension table or event

        await sessionRepository.Save(session, cancellationToken);

        return Result.Success(new JoinSessionResult(session, participant));
    }
}

