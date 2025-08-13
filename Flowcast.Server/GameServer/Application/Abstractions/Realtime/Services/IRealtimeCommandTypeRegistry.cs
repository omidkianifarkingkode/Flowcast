using Application.Abstractions.Realtime.Messaging;

namespace Application.Abstractions.Realtime.Services;

public interface IRealtimeCommandTypeRegistry
{
    Type? TryGetCommandType(RealtimeMessageType type);
}