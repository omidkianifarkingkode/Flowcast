using Application.Sessions.Shared;
using Realtime.Transport.Messaging.Sender;
using Session.Contracts;
using Session.Domain;
using Shared.Application.Services;
using SharedKernel.Primitives;

namespace Session.Infrastructure;

public sealed class SessionNotifier(IRealtimeMessageSender messenger, IDateTimeProvider clock)
    : ISessionNotifier
{
    public async Task SessionCreated(SessionEntity s, CancellationToken ct)
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
