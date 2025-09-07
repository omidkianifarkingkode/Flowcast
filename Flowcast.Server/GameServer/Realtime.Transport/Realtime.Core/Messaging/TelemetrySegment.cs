using System.Buffers.Binary;

namespace Realtime.Transport.Messaging;

[Flags]
public enum HeaderFlags : byte
{
    None = 0,
    HasTelemetry = 1 << 0,
    IsCompressed = 1 << 1,
    IsEncrypted = 1 << 2,
    IsBatch = 1 << 3,
    HasExt = 1 << 4
}

/// <summary>
/// Variable-length telemetry segment encoded as:
/// [2:len][ TLVs... ] where TLV = [2:key][2:len][N:value]
/// NOTE: The outer [2:len] is written/read by the codec; this type
/// writes/reads only the TLV content portion.
/// </summary>
public readonly struct TelemetrySegment(IEnumerable<TelemetryField> fields)
{
    public readonly IReadOnlyList<TelemetryField> Fields = fields.ToList();

    public int GetContentSize()
        => Fields.Sum(f => 2 + 2 + (f.Value?.Length ?? 0));

    public void WriteContentTo(Span<byte> buffer)
    {
        var offs = 0;
        foreach (var f in Fields)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(offs, 2), f.Key); offs += 2;
            var len = (ushort)(f.Value?.Length ?? 0);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(offs, 2), len); offs += 2;
            if (len > 0)
            {
                f.Value!.CopyTo(buffer.Slice(offs, len));
                offs += len;
            }
        }
    }

    public static TelemetrySegment ReadContentFrom(ReadOnlySpan<byte> buffer)
    {
        var fields = new List<TelemetryField>(capacity: 4);
        var offs = 0;
        while (offs + 4 <= buffer.Length)
        {
            var key = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offs, 2)); offs += 2;
            var len = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offs, 2)); offs += 2;
            if (offs + len > buffer.Length) throw new ArgumentException("Telemetry TLV truncated.");
            var val = len == 0 ? [] : buffer.Slice(offs, len).ToArray();
            offs += len;
            fields.Add(new TelemetryField(key, val));
        }
        if (offs != buffer.Length) throw new ArgumentException("Telemetry trailing bytes.");
        return new TelemetrySegment(fields);
    }

    public override string ToString() => $"Telemetry[{Fields.Count} fields]";
}

public readonly struct TelemetryField(ushort key, byte[] value)
{
    public ushort Key { get; } = key;
    public byte[] Value { get; } = value ?? Array.Empty<byte>();
}
