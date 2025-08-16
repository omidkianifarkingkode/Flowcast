namespace Application.Realtime.Services;

public interface IRealtimeMessageReceiver
{
    Task ReceiveTextMessage(Guid userId, string message, CancellationToken cancellationToken = default);
    Task ReceiveBinaryMessage(Guid userId, byte[] data, CancellationToken cancellationToken = default);
}

