using MessagePack;
using Realtime.Transport.Messaging.Codec;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;

namespace Realtime.Transport.Messaging.Factories;

public interface IRealtimePayloadFactory
{
    // Non-generic (socket entry)
    IRealtimeMessage CreateFromJson(string json);
    IRealtimeMessage CreateFromBinary(byte[] data);

    // Typed convenience
    RealtimeMessage<TPayload> CreateFromJson<TPayload>(string json) where TPayload : IPayload;
    RealtimeMessage<TPayload> CreateFromBinary<TPayload>(byte[] data) where TPayload : IPayload;
}

public sealed class RealtimePayloadFactory : IRealtimePayloadFactory
{
    private readonly ICodec _codec;
    private readonly IRealtimePayloadTypeRegistry _registry;

    public RealtimePayloadFactory(
        ICodec codec,
        IRealtimePayloadTypeRegistry registry)
    {
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    // ---------------- Non-generic (JSON) ----------------
    public IRealtimeMessage CreateFromJson(string json)
    {
        // Parse header first (no payload materialization)
        var headerOnly = _codec.FromJsonHeaderOnly(json);

        // If message type isn't mapped to a command, it's header-only (Ping/Pong)
        var cmdType = _registry.TryGetPayloadType(headerOnly.Header.Type);
        if (cmdType is null)
            return headerOnly;

        // Decode payload into the discovered command type via cached delegate
        var decode = CodecInvokerCache.GetJsonDecoder(cmdType);
        return decode(_codec, json);
    }

    // ---------------- Non-generic (Binary) ----------------
    public IRealtimeMessage CreateFromBinary(byte[] data)
    {
        // Parse header first
        var headerOnly = _codec.FromBytesHeaderOnly(data);

        var cmdType = _registry.TryGetPayloadType(headerOnly.Header.Type);
        if (cmdType is null)
            return headerOnly;

        var decode = CodecInvokerCache.GetBinaryDecoder(cmdType);
        return decode(_codec, data);
    }

    // ---------------- Typed convenience ----------------
    public RealtimeMessage<TPayload> CreateFromJson<TPayload>(string json)
        where TPayload : IPayload
        => _codec.FromJson<TPayload>(json);

    public RealtimeMessage<TPayload> CreateFromBinary<TPayload>(byte[] data)
        where TPayload : IPayload
        => _codec.FromBytes<TPayload>(data);

    // ---------- Cached invokers to the codec's generic methods ----------
    private static class CodecInvokerCache
    {
        // IRealtimeMessage FromJson<T>(codec, json)
        private static readonly ConcurrentDictionary<Type, Func<ICodec, string, IRealtimeMessage>>
            _jsonDecoders = new();

        // IRealtimeMessage FromBytes<T>(codec, data)
        private static readonly ConcurrentDictionary<Type, Func<ICodec, byte[], IRealtimeMessage>>
            _binaryDecoders = new();

        public static Func<ICodec, string, IRealtimeMessage> GetJsonDecoder(Type payloadType)
            => _jsonDecoders.GetOrAdd(payloadType, BuildJsonDecoder);

        public static Func<ICodec, byte[], IRealtimeMessage> GetBinaryDecoder(Type payloadType)
            => _binaryDecoders.GetOrAdd(payloadType, BuildBinaryDecoder);

        private static Func<ICodec, string, IRealtimeMessage> BuildJsonDecoder(Type payloadType)
        {
            // Build: (codec, json) => (IRealtimeMessage) codec.FromJson<T>(json, null)
            var iface = typeof(ICodec);
            var open = iface.GetMethod(nameof(ICodec.FromJson))!; // generic
            var closed = open.MakeGenericMethod(payloadType);

            var pCodec = Expression.Parameter(typeof(ICodec), "codec");
            var pJson = Expression.Parameter(typeof(string), "json");

            var call = Expression.Call(
                pCodec,
                closed,
                pJson,
                Expression.Constant(null, typeof(JsonSerializerOptions)));

            var cast = Expression.Convert(call, typeof(IRealtimeMessage));

            return Expression.Lambda<Func<ICodec, string, IRealtimeMessage>>(cast, pCodec, pJson)
                             .Compile();
        }

        private static Func<ICodec, byte[], IRealtimeMessage> BuildBinaryDecoder(Type payloadType)
        {
            // Build: (codec, data) => (IRealtimeMessage) codec.FromBytes<T>(data, null)
            var iface = typeof(ICodec);
            var open = iface.GetMethod(nameof(ICodec.FromBytes))!; // generic
            var closed = open.MakeGenericMethod(payloadType);

            var pCodec = Expression.Parameter(typeof(ICodec), "codec");
            var pData = Expression.Parameter(typeof(byte[]), "data");

            var call = Expression.Call(
                pCodec,
                closed,
                pData,
                Expression.Constant(null, typeof(MessagePackSerializerOptions)));

            var cast = Expression.Convert(call, typeof(IRealtimeMessage));

            return Expression.Lambda<Func<ICodec, byte[], IRealtimeMessage>>(cast, pCodec, pData)
                             .Compile();
        }
    }
}
