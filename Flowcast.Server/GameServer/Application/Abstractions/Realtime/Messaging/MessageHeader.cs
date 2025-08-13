using System.Buffers.Binary;

namespace Application.Abstractions.Realtime.Messaging;

public readonly struct MessageHeader(RealtimeMessageType type, ulong id, long timestamp)
{
    public RealtimeMessageType Type { get; } = type;
    public ulong Id { get; } = id;
    public long Timestamp { get; } = timestamp;

    // Big-endian header: [2:type][8:id][8:timestamp] = 18 bytes
    public const int Size = 2 + 8 + 8;

    public void WriteTo(Span<byte> buffer)
    {
        if (buffer.Length < Size) throw new ArgumentException("Buffer too small.", nameof(buffer));

        BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(0, 2), (short)Type);
        BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(2, 8), Id);
        BinaryPrimitives.WriteInt64BigEndian(buffer.Slice(10, 8), Timestamp);
    }

    public static MessageHeader ReadFrom(ReadOnlySpan<byte> span)
    {
        if (span.Length < Size) throw new ArgumentException("Span too small.", nameof(span));

        var type = (RealtimeMessageType)BinaryPrimitives.ReadInt16BigEndian(span.Slice(0, 2));
        var id = BinaryPrimitives.ReadUInt64BigEndian(span.Slice(2, 8));
        var timestamp = BinaryPrimitives.ReadInt64BigEndian(span.Slice(10, 8));
        return new MessageHeader(type, id, timestamp);
    }
}
