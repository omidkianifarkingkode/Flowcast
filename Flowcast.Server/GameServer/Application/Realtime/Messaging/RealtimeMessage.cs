using System.Reflection;

namespace Application.Realtime.Messaging;

// Non-generic base for all incoming messages (header only).
public interface IRealtimeMessage
{
    MessageHeader Header { get; }
}

// Messages that *do* carry a command payload expose it via this interface.
public interface IRealtimePayloadMessage : IRealtimeMessage
{
    IRealtimeCommand Payload { get; }
}

// Header-only (no payload) — e.g., Ping/Pong
public readonly struct RealtimeMessage(MessageHeader header) : IRealtimeMessage
{
    public MessageHeader Header { get; } = header;

    public static RealtimeMessage Create(
        RealtimeMessageType type,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp, flags, telemetry));

    public override string ToString()
        => $"RealtimeMessage {{ {Header} }}";
}

// Header + typed payload
public readonly struct RealtimeMessage<TCommand>(MessageHeader header, TCommand payload) : IRealtimePayloadMessage
    where TCommand : IRealtimeCommand
{
    public MessageHeader Header { get; } = header;
    public TCommand Payload { get; } = payload ?? throw new ArgumentNullException(nameof(payload));

    IRealtimeCommand IRealtimePayloadMessage.Payload => Payload;

    // Cache attribute lookup once per closed generic
    private static readonly RealtimeMessageType? _cachedType =
        typeof(TCommand).GetCustomAttribute<RealtimeMessageAttribute>()?.MessageType;

    public static RealtimeMessage<TCommand> Create(
        RealtimeMessageType type,
        TCommand payload,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp, flags, telemetry), payload);

    public static RealtimeMessage<TCommand> Create(
        TCommand payload,
        long timestamp,
        ulong? id = null,
        HeaderFlags flags = HeaderFlags.None,
        TelemetrySegment? telemetry = null)
        => _cachedType is RealtimeMessageType mt
            ? Create(mt, payload, timestamp, id, flags, telemetry)
            : throw new InvalidOperationException(
                $"Type {typeof(TCommand).Name} is missing [RealtimeMessageAttribute]. " +
                "Use the overload that accepts RealtimeMessageType explicitly, or add the attribute.");

    public override string ToString()
    {
        // Keep it concise: show payload type; avoid dumping full object graphs by default.
        var payloadType = Payload?.GetType().Name ?? typeof(TCommand).Name;
        return $"RealtimeMessage<{payloadType}> {{ {Header} }}";
    }
}