using Application.Realtime.Messaging;

namespace Application.Realtime.Services;

public interface IRealtimeCommandFactory
{
    // Non-generic (socket entry)
    IRealtimeMessage CreateFromJson(string json);
    IRealtimeMessage CreateFromBinary(byte[] data);

    // Typed convenience
    RealtimeMessage<TCommand> CreateFromJson<TCommand>(string json) where TCommand : IRealtimeCommand;
    RealtimeMessage<TCommand> CreateFromBinary<TCommand>(byte[] data) where TCommand : IRealtimeCommand;
}
