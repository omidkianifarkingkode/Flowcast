using System.Buffers.Binary;
using System.Text;

namespace Application.Abstractions.Realtime;

public readonly struct RealtimeMessage
{
    public RealtimeMessageType Type { get; }
    public ulong Id { get; }
    public long Timestamp { get; }
    public byte[] Payload { get; }

    // 1 byte type + 16 bytes Guid + 8 bytes timestamp
    public const int HeaderSize = 1 + 16 + 8;

    private static long _counter = 0;

    public RealtimeMessage(RealtimeMessageType messageType, ulong messageId, long timestamp, byte[] payload)
    {
        Type = messageType;
        Id = messageId;
        Timestamp = timestamp;
        Payload = payload ?? [];
    }

    public byte[] ToBytes()
    {
        var buffer = new byte[HeaderSize + Payload.Length];
        buffer[0] = (byte)Type;

        // Write ulong MessageId (big-endian)
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(1, 8), Id);

        // Write timestamp (big-endian)
        BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(17, 8), Timestamp);

        // Write payload
        if (Payload.Length > 0)
            Buffer.BlockCopy(Payload, 0, buffer, HeaderSize, Payload.Length);

        return buffer;
    }

    public static RealtimeMessage FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize)
            throw new ArgumentException("Invalid message length", nameof(data));

        var type = (RealtimeMessageType)data[0];

        ulong messageId = BinaryPrimitives.ReadUInt64BigEndian(data.Slice(1, 8));

        var timestamp = BinaryPrimitives.ReadInt64BigEndian(data.Slice(17, 8));

        var payload = data.Length > HeaderSize
            ? data.Slice(HeaderSize).ToArray()
            : [];

        return new RealtimeMessage(type, messageId, timestamp, payload);
    }

    public static RealtimeMessage Create(
        RealtimeMessageType type,
        long timestamp,
        byte[] payload)
    {
        return new RealtimeMessage(
            type,
            GenerateId(),
            timestamp,
            payload);
    }

    public string GetPayloadAsString()
    {
        return Encoding.UTF8.GetString(Payload);
    }

    private static ulong GenerateId()
    {
        return (ulong)Interlocked.Increment(ref _counter);
    }

    public override string ToString()
    {
        return $"[{Type}] Id={Id} Time={Timestamp} PayloadLength={Payload.Length}";
    }
}
