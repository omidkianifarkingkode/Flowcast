using System.Reflection;

namespace Application.Abstractions.Realtime.Messaging;

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

    public static RealtimeMessage Create(RealtimeMessageType type, long timestamp, ulong? id = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp));
}

// Header + typed payload
public readonly struct RealtimeMessage<TCommand>(MessageHeader header, TCommand payload) : IRealtimePayloadMessage
    where TCommand : IRealtimeCommand
{
    public MessageHeader Header { get; } = header;
    public TCommand Payload { get; } = payload ?? throw new ArgumentNullException(nameof(payload));

    IRealtimeCommand IRealtimePayloadMessage.Payload => Payload;

    // NEW: explicit type (use when you know the RealtimeMessageType at callsite)
    public static RealtimeMessage<TCommand> Create(
        RealtimeMessageType type,
        TCommand payload,
        long timestamp,
        ulong? id = null)
        => new(new MessageHeader(type, id ?? IdGenerator.NewId(), timestamp), payload);

    // NEW: infer type from the command's [RealtimeMessage(MessageType = ...)] attribute
    public static RealtimeMessage<TCommand> Create(
        TCommand payload,
        long timestamp,
        ulong? id = null)
    {
        var attr = typeof(TCommand).GetCustomAttribute<RealtimeMessageAttribute>();
        if (attr is null)
            throw new InvalidOperationException(
                $"Type {typeof(TCommand).Name} is missing [RealtimeMessageAttribute]. " +
                "Use the overload that accepts RealtimeMessageType explicitly, or add the attribute.");
        return Create(attr.MessageType, payload, timestamp, id);
    }
}