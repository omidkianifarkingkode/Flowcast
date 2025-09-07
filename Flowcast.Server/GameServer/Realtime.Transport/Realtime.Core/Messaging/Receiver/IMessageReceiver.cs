using Realtime.Transport.Http;

namespace Realtime.Transport.Messaging.Receiver;

public interface IMessageReceiver
{
    event Action<RealtimeContext, IRealtimeMessage> OnMessageReceived;

    Task ReceiveTextMessage(string userId, string message, CancellationToken cancellationToken = default);
    Task ReceiveBinaryMessage(string userId, byte[] data, CancellationToken cancellationToken = default);
}

