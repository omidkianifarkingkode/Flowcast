using Application.Realtime.Messaging;

namespace Application.Realtime.Services;

public interface IRealtimeMessageSender
{
    Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default);
    Task SendToUserAsync<T>(Guid userId, RealtimeMessage<T> message, CancellationToken cancellationToken = default)
        where T : IRealtimeCommand;
}