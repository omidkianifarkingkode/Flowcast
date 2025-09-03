using Realtime.Transport.Http;

namespace Realtime.Transport.Messaging.Sender;

public interface IRealtimeMessageSender
{
    Task SendToUserAsync(string userId, RealtimeMessage message, CancellationToken cancellationToken = default);

    Task SendToUserAsync<TPayload>(string userId, RealtimeMessage<TPayload> message, CancellationToken cancellationToken = default)
        where TPayload : IPayload;

    Task SendToUserAsync<TPayload>(string userId, ushort type, TPayload payload, CancellationToken cancellationToken = default)
        where TPayload : IPayload;
}