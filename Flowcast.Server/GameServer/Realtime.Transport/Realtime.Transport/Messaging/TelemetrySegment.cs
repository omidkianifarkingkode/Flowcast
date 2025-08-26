using System.Buffers.Binary;

namespace Realtime.Transport.Messaging;

[Flags]
public enum HeaderFlags : byte
{
    None = 0,
    HasTelemetry = 1
}

public readonly struct TelemetrySegment(ulong lastPingId, int lastRttMs, long clientSendTs)
{
    public ulong LastPingId { get; } = lastPingId;   // correlation to a ping id
    public int LastRttMs { get; } = lastRttMs;    // server-measured RTT to this peer
    public long ClientSendTs { get; } = clientSendTs; // client-sent timestamp (optional)

    // [8:LastPingId][4:LastRttMs][8:ClientSendTs] = 20 bytes
    public const int Size = 8 + 4 + 8;

    public void WriteTo(Span<byte> buffer)
    {
        if (buffer.Length < Size) throw new ArgumentException("Buffer too small.", nameof(buffer));
        BinaryPrimitives.WriteUInt64BigEndian(buffer[0..8], LastPingId);       // [0..7]
        BinaryPrimitives.WriteInt32BigEndian(buffer[8..12], LastRttMs);       // [8..11]
        BinaryPrimitives.WriteInt64BigEndian(buffer[12..20], ClientSendTs);   // [12..19]
    }

    public static TelemetrySegment ReadFrom(ReadOnlySpan<byte> span)
    {
        if (span.Length < Size) throw new ArgumentException("Span too small.", nameof(span));
        var lastPingId = BinaryPrimitives.ReadUInt64BigEndian(span[0..8]);
        var lastRttMs = BinaryPrimitives.ReadInt32BigEndian(span[8..12]);
        var clientSendTs = BinaryPrimitives.ReadInt64BigEndian(span[12..20]);
        return new TelemetrySegment(lastPingId, lastRttMs, clientSendTs);
    }

    public override string ToString()
    {
        static string Iso(long ms)
            => DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime.ToString("O");

        var clientTsPart = ClientSendTs != 0
            ? $"{ClientSendTs} ({Iso(ClientSendTs)})"
            : "0";

        return $"LastPingId={LastPingId}, LastRttMs={LastRttMs}, ClientSendTs={clientTsPart}";
    }
}
