namespace Realtime.Transport.Messaging.Receiver;

public interface IRealtimeMessageReceiver
{
    Task ReceiveTextMessage(string userId, string message, CancellationToken cancellationToken = default);
    Task ReceiveBinaryMessage(string userId, byte[] data, CancellationToken cancellationToken = default);
}

