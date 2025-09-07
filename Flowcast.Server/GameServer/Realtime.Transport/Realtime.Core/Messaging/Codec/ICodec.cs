using MessagePack;
using System.Text.Json;

namespace Realtime.Transport.Messaging.Codec;

public interface ICodec
{
    // Typed payload
    byte[] ToBytes<TPayload>(RealtimeMessage<TPayload> message, MessagePackSerializerOptions? serializerOptions = null)
        where TPayload : IPayload;
    RealtimeMessage<TPayload> FromBytes<TPayload>(byte[] data, MessagePackSerializerOptions? serializerOptions = null)
        where TPayload : IPayload;

    byte[] ToJson<TPayload>(RealtimeMessage<TPayload> message, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload;
    RealtimeMessage<TPayload> FromJson<TPayload>(string json, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload;

    // Header-only (Ping/Pong)
    byte[] ToBytes(RealtimeMessage message);
    RealtimeMessage FromBytesHeaderOnly(byte[] data);

    byte[] ToJson(RealtimeMessage message, JsonSerializerOptions? serializerOptions = null);
    RealtimeMessage FromJsonHeaderOnly(string json, JsonSerializerOptions? serializerOptions = null);
}