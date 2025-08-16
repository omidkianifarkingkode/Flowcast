using System.Buffers.Binary;

namespace Application.Realtime.Messaging;

public readonly struct MessageHeader(
    RealtimeMessageType type,
    ulong id,
    long timestamp,
    HeaderFlags flags = HeaderFlags.None,
    TelemetrySegment? telemetry = null)
{
    public RealtimeMessageType Type { get; } = type;
    public ulong Id { get; } = id;
    public long Timestamp { get; } = timestamp;
    public HeaderFlags Flags { get; } = flags;
    public TelemetrySegment? Telemetry { get; } = telemetry;

    // Big-endian header (fixed part only): [2:type][8:id][8:timestamp][1:flags] = 19 bytes
    public const int Size = 2 + 8 + 8 + 1;

    public void WriteTo(Span<byte> buffer)
    {
        if (buffer.Length < Size) throw new ArgumentException("Buffer too small.", nameof(buffer));
        BinaryPrimitives.WriteInt16BigEndian(buffer[0..2], (short)Type);   // [0..1]
        BinaryPrimitives.WriteUInt64BigEndian(buffer[2..10], Id);           // [2..9]
        BinaryPrimitives.WriteInt64BigEndian(buffer[10..18], Timestamp);    // [10..17]
        buffer[18] = (byte)Flags;                                           // [18]
        // NOTE: Telemetry is an optional segment that follows the header in the binary frame.
    }

    public static MessageHeader ReadFrom(ReadOnlySpan<byte> span)
    {
        if (span.Length < Size) throw new ArgumentException("Span too small.", nameof(span));
        var type = (RealtimeMessageType)BinaryPrimitives.ReadInt16BigEndian(span[0..2]);
        var id = BinaryPrimitives.ReadUInt64BigEndian(span[2..10]);
        var timestamp = BinaryPrimitives.ReadInt64BigEndian(span[10..18]);
        var flags = (HeaderFlags)span[18];
        return new(type, id, timestamp, flags, telemetry: null);
    }

    // Helper to attach telemetry after parsing it
    public MessageHeader WithTelemetry(TelemetrySegment telemetry)
        => new(Type, Id, Timestamp, Flags, telemetry);

    public override string ToString()
    {
        static string Iso(long ms)
            => DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime.ToString("O");

        var basePart = $"Type={Type}, Id={Id}, Time={Timestamp} ({Iso(Timestamp)}), Flags={Flags}";

        if ((Flags & HeaderFlags.HasTelemetry) != 0 && Telemetry.HasValue)
        {
            return $"{basePart}, Telemetry({Telemetry.Value})";
        }

        return basePart;
    }
}
