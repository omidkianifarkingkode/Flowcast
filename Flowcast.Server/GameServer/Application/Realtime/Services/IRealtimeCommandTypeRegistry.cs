using Application.Realtime.Messaging;

namespace Application.Realtime.Services;

public interface IRealtimeCommandTypeRegistry
{
    Type? TryGetCommandType(RealtimeMessageType type);
}