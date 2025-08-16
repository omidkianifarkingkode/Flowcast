using Application.Realtime.Messaging;
using Application.Realtime.Services;
using MessagePack;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;

namespace Infrastructure.Realtime.Services;

public sealed class RealtimeCommandFactory : IRealtimeCommandFactory
{
    private readonly IRealtimeMessageCodec _codec;
    private readonly IRealtimeCommandTypeRegistry _registry;

    public RealtimeCommandFactory(
        IRealtimeMessageCodec codec,
        IRealtimeCommandTypeRegistry registry)
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
        var cmdType = _registry.TryGetCommandType(headerOnly.Header.Type);
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

        var cmdType = _registry.TryGetCommandType(headerOnly.Header.Type);
        if (cmdType is null)
            return headerOnly;

        var decode = CodecInvokerCache.GetBinaryDecoder(cmdType);
        return decode(_codec, data);
    }

    // ---------------- Typed convenience ----------------
    public RealtimeMessage<TCommand> CreateFromJson<TCommand>(string json)
        where TCommand : IRealtimeCommand
        => _codec.FromJson<TCommand>(json);

    public RealtimeMessage<TCommand> CreateFromBinary<TCommand>(byte[] data)
        where TCommand : IRealtimeCommand
        => _codec.FromBytes<TCommand>(data);

    // ---------- Cached invokers to the codec's generic methods ----------
    private static class CodecInvokerCache
    {
        // IRealtimeMessage FromJson<T>(codec, json)
        private static readonly ConcurrentDictionary<Type, Func<IRealtimeMessageCodec, string, IRealtimeMessage>>
            _jsonDecoders = new();

        // IRealtimeMessage FromBytes<T>(codec, data)
        private static readonly ConcurrentDictionary<Type, Func<IRealtimeMessageCodec, byte[], IRealtimeMessage>>
            _binaryDecoders = new();

        public static Func<IRealtimeMessageCodec, string, IRealtimeMessage> GetJsonDecoder(Type payloadType)
            => _jsonDecoders.GetOrAdd(payloadType, BuildJsonDecoder);

        public static Func<IRealtimeMessageCodec, byte[], IRealtimeMessage> GetBinaryDecoder(Type payloadType)
            => _binaryDecoders.GetOrAdd(payloadType, BuildBinaryDecoder);

        private static Func<IRealtimeMessageCodec, string, IRealtimeMessage> BuildJsonDecoder(Type payloadType)
        {
            // Build: (codec, json) => (IRealtimeMessage) codec.FromJson<T>(json, null)
            var iface = typeof(IRealtimeMessageCodec);
            var open = iface.GetMethod(nameof(IRealtimeMessageCodec.FromJson))!; // generic
            var closed = open.MakeGenericMethod(payloadType);

            var pCodec = Expression.Parameter(typeof(IRealtimeMessageCodec), "codec");
            var pJson = Expression.Parameter(typeof(string), "json");

            var call = Expression.Call(
                pCodec,
                closed,
                pJson,
                Expression.Constant(null, typeof(JsonSerializerOptions)));

            var cast = Expression.Convert(call, typeof(IRealtimeMessage));

            return Expression.Lambda<Func<IRealtimeMessageCodec, string, IRealtimeMessage>>(cast, pCodec, pJson)
                             .Compile();
        }

        private static Func<IRealtimeMessageCodec, byte[], IRealtimeMessage> BuildBinaryDecoder(Type payloadType)
        {
            // Build: (codec, data) => (IRealtimeMessage) codec.FromBytes<T>(data, null)
            var iface = typeof(IRealtimeMessageCodec);
            var open = iface.GetMethod(nameof(IRealtimeMessageCodec.FromBytes))!; // generic
            var closed = open.MakeGenericMethod(payloadType);

            var pCodec = Expression.Parameter(typeof(IRealtimeMessageCodec), "codec");
            var pData = Expression.Parameter(typeof(byte[]), "data");

            var call = Expression.Call(
                pCodec,
                closed,
                pData,
                Expression.Constant(null, typeof(MessagePackSerializerOptions)));

            var cast = Expression.Convert(call, typeof(IRealtimeMessage));

            return Expression.Lambda<Func<IRealtimeMessageCodec, byte[], IRealtimeMessage>>(cast, pCodec, pData)
                             .Compile();
        }
    }
}
