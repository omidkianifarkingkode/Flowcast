using Application.Realtime.Messaging;
using MessagePack;
using System.Text.Json;

namespace Application.Realtime.Services;

public interface IRealtimeMessageCodec
{
    // Typed payload
    byte[] ToBytes<TCommand>(RealtimeMessage<TCommand> message, MessagePackSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand;
    RealtimeMessage<TCommand> FromBytes<TCommand>(byte[] data, MessagePackSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand;

    string ToJson<TCommand>(RealtimeMessage<TCommand> message, JsonSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand;
    RealtimeMessage<TCommand> FromJson<TCommand>(string json, JsonSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand;

    // Header-only (Ping/Pong)
    byte[] ToBytes(RealtimeMessage message);
    RealtimeMessage FromBytesHeaderOnly(byte[] data);

    string ToJson(RealtimeMessage message, JsonSerializerOptions? serializerOptions = null);
    RealtimeMessage FromJsonHeaderOnly(string json, JsonSerializerOptions? serializerOptions = null);
}
