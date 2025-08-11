namespace Application.Abstractions.Realtime;

public interface IRealtimeMessageReceiver
{
    Task OnMessageReceivedAsync(Guid userId, string message, CancellationToken cancellationToken = default);
    Task OnMessageReceivedAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default);
}

