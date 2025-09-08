using Application.Sessions.Shared;
using Domain.Sessions;
using Realtime.Transport.Messaging.Sender;
using SharedKernel;

namespace Infrastructure.Persistence.Seasons;

public sealed class RealtimeSessionNotifier(IRealtimeMessageSender messenger, IDateTimeProvider clock)
    : ISessionNotifier
{
    public async Task SessionCreated(Session s, CancellationToken ct)
    {
        var payload = new SessionCreatedCommand
        {
            SessionId = s.Id.Value,
            Mode = s.Mode,
            StartBarrier = s.Barrier.ToString(),
            CreatedAtUtc = s.CreatedAtUtc,
            JoinDeadlineUtc = s.JoinDeadlineUtc,
            Participants = s.Participants.Select(p => p.Id.Value).ToArray()
        };

        // Push to all participants
        foreach (var p in s.Participants)
        {
            await messenger.SendToUserAsync(
                p.Id.Value.ToString("D"),
                SessionCreatedCommand.Type,
                payload,
                ct);
        }
    }

    public async Task SessionCreateFail(string reasonCode, string message, PlayerId[] recipients, CancellationToken ct)
    {
        var payload = new SessionCreateFailCommand { ReasonCode = reasonCode, Message = message };
        foreach (var pid in recipients)
        {
            await messenger.SendToUserAsync(
                pid.Value.ToString("D"),
                SessionCreateFailCommand.Type,
                payload,
                ct);
        }
    }
}
