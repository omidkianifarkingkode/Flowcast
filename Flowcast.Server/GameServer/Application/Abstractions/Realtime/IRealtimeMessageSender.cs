namespace Application.Abstractions.Realtime;

public interface IRealtimeMessageSender
{
    Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default);
}