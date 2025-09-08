using Application.Abstractions.Messaging;
using Application.Sessions.Shared;
using Domain.Sessions;
using MessagePack;
using Realtime.Transport.Messaging;
using SharedKernel;

namespace Application.Sessions.Commands;

[MessagePackObject]
[RealtimeMessage(SessionV1.Create)]
public record CreateSessionCommand : ICommand<SessionId>, IPayload
{
    [Key(0)] public List<PlayerInfo> Players { get; set; } = new();
    [Key(1)] public string Mode { get; set; } = "";
    [Key(2)] public MatchSettings? MatchSettings { get; set; }
    [Key(3)] public string StartBarrier { get; set; } = "ConnectedAndLoaded";
    [Key(4)] public int? JoinDeadlineSeconds { get; set; } = 15;

    [MessagePackObject]
    public record PlayerInfo(
        [property: Key(0)] Guid Id,
        [property: Key(1)] string DisplayName);

    public CreateSessionCommand(
        List<PlayerInfo> players,
        string mode,
        MatchSettings? matchSettings,
        string startBarrier = "ConnectedAndLoaded",
        int? joinDeadlineSeconds = 15)
    {
        Players = players ?? new();
        Mode = mode;
        MatchSettings = matchSettings;
        StartBarrier = startBarrier;
        JoinDeadlineSeconds = joinDeadlineSeconds;
    }
}

public sealed class CreateSessionHandler(ISessionRepository sessionRepository, ISessionNotifier notifier, IDateTimeProvider clock)
    : ICommandHandler<CreateSessionCommand, SessionId>
{
    public async Task<Result<SessionId>> Handle(CreateSessionCommand command, CancellationToken ct)
        => await CreateInternalAsync(command, ct);

    private async Task<Result<SessionId>> CreateInternalAsync(CreateSessionCommand cmd, CancellationToken ct)
    {
        // Basic validation
        if (cmd.Players is null || cmd.Players.Count == 0)
            return await FailAsync("session.no_players", "At least one player is required.", cmd, ct);
        
        if (!Enum.TryParse<StartBarrier>(cmd.StartBarrier, true, out var barrier))
            return await FailAsync("session.invalid_barrier", $"Unknown start barrier '{cmd.StartBarrier}'.", cmd, ct);

        // Decide join deadline policy:
        // Keep your previous behavior: give a deadline for Timer / ConnectedOnly / ConnectedAndLoaded
        var joinDeadline = (cmd.JoinDeadlineSeconds is int s && s > 0)
            ? clock.UtcNow.AddSeconds(s)
            : clock.UtcNow.AddSeconds(15);

        var session = Session.Create(cmd.Mode, barrier, clock.UtcNow,
            joinDeadlineUtc: joinDeadline, settings: cmd.MatchSettings);

        // Add participants
        foreach (var p in cmd.Players)
        {
            var participant = new Participant(new PlayerId(p.Id), p.DisplayName);
            var joinRes = session.JoinParticipant(participant, clock.UtcNow);
            if (joinRes.IsFailure)
                return await FailAsync(joinRes.Error.Code, joinRes.Error.Description, cmd, ct);
        }

        await sessionRepository.Save(session, ct);

        // Success push
        await notifier.SessionCreated(session, ct);
        return session.Id;
    }

    private async Task<Result<SessionId>> FailAsync(string code, string message, CreateSessionCommand cmd, CancellationToken ct)
    {
        var recipients = cmd.Players.Select(p => new PlayerId(p.Id)).ToArray();
        await notifier.SessionCreateFail(code, message, recipients, ct);
        return Result.Failure<SessionId>(Error.Problem(code, message));
    }
}


