using Realtime.Transport.Messaging.Factories;
using System.Reflection;

namespace Realtime.Transport.Messaging;

public static class RealtimeMessageType
{
    public const ushort Ping = 3;
    public const ushort Pong = 4;
}

// Non-generic base for all incoming messages (header only).
public interface IRealtimeMessage
{
    MessageHeader Header { get; }
}

public interface IPayload { }

// Messages that *do* carry a command payload expose it via this interface.
public interface IRealtimePayloadMessage : IRealtimeMessage
{
    IPayload Payload { get; }
}

// Header-only (no payload) — e.g., Ping/Pong
public readonly struct RealtimeMessage(MessageHeader header) : IRealtimeMessage
{
    public MessageHeader Header { get; } = header;

    public static RealtimeMessage Create(
        ushort type,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp, flags, telemetry));

    public override string ToString()
        => $"RealtimeMessage {{ {Header} }}";
}

// Header + typed payload
public readonly struct RealtimeMessage<TPayload>(MessageHeader header, TPayload payload) : IRealtimePayloadMessage
    where TPayload : IPayload
{
    public MessageHeader Header { get; } = header;
    public TPayload Payload { get; } = payload ?? throw new ArgumentNullException(nameof(payload));

    IPayload IRealtimePayloadMessage.Payload => Payload;

    // Cache attribute lookup once per closed generic
    private static readonly ushort? _cachedType =
        typeof(TPayload).GetCustomAttribute<RealtimeMessageAttribute>()?.MessageType;

    public static RealtimeMessage<TPayload> Create(
        ushort type,
        TPayload payload,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp, flags, telemetry), payload);

    public static RealtimeMessage<TPayload> Create(
        TPayload payload,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => _cachedType is ushort mt
            ? Create(mt, payload, timestamp, id, flags, telemetry)
            : throw new InvalidOperationException(
                $"Type {typeof(TPayload).Name} is missing [RealtimeMessageAttribute]. " +
                "Use the overload that accepts RealtimeMessageType explicitly, or add the attribute.");

    public override string ToString()
    {
        // Keep it concise: show payload type; avoid dumping full object graphs by default.
        var payloadType = Payload?.GetType().Name ?? typeof(TPayload).Name;
        return $"RealtimeMessage<{payloadType}> {{ {Header} }}";
    }
}