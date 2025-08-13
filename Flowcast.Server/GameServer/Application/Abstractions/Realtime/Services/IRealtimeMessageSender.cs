using Application.Abstractions.Realtime.Messaging;

namespace Application.Abstractions.Realtime.Services;

public interface IRealtimeMessageSender
{
    Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default);
}