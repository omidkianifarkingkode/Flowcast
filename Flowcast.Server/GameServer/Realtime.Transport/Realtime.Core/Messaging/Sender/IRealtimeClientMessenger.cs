using Realtime.Transport.Http;

namespace Realtime.Transport.Messaging.Sender;

public interface IRealtimeClientMessenger
{
    Task SendAsync<TPayload>(ushort type, TPayload payload, CancellationToken cancellationToken = default)
        where TPayload : IPayload;
}

/// <summary>
/// Scoped helper for application handlers. Sends to the *current* user (from RealtimeContextAccessor).
/// </summary>
public sealed class RealtimeClientMessenger(
    IRealtimeMessageSender messageSender,
    IRealtimeContextAccessor realtimeContextAccessor)
    : IRealtimeClientMessenger
{
    private readonly IRealtimeMessageSender _messageSender = messageSender;
    private readonly IRealtimeContextAccessor _contextAccessor = realtimeContextAccessor;

    public Task SendAsync<TPayload>(ushort type, TPayload payload, CancellationToken cancellationToken = default)
        where TPayload : IPayload
    {
        var context = _contextAccessor.Current
            ?? throw new InvalidOperationException("No RealtimeContext available. Are you calling from within a routed handler?");

        return _messageSender.SendToUserAsync(context.UserId, type, payload, cancellationToken);
    }
}
