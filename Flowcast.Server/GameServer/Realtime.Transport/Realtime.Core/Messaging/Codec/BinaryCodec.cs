using MessagePack;
using System.Buffers;
using System.Buffers.Binary;

namespace Realtime.Transport.Messaging.Codec;

public sealed partial class Codec
{
    public byte[] ToBytes<TPayload>(RealtimeMessage<TPayload> message, MessagePackSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        serializerOptions ??= _messagePackSerializerOptions;

        var payloadBytes = MessagePackSerializer.Serialize(message.Payload, serializerOptions);

        var hasTelemetry = (message.Header.Flags & HeaderFlags.HasTelemetry) != 0
                       && message.Header.Telemetry.HasValue;
        var telemetryContentLen = hasTelemetry ? message.Header.Telemetry!.Value.GetContentSize() : 0;
        var telemetryTotalLen = hasTelemetry ? (2 + telemetryContentLen) : 0; // 2 = length prefix

        var total = MessageHeader.Size + telemetryTotalLen + payloadBytes.Length;
        var buffer = new byte[total];

        message.Header.WriteTo(buffer);

        var offset = MessageHeader.Size;

        // telemetry (len + TLVs)
        if (hasTelemetry)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), (ushort)telemetryContentLen);
            offset += 2;
            message.Header.Telemetry!.Value.WriteContentTo(buffer.AsSpan(offset, telemetryContentLen));
            offset += telemetryContentLen;
        }

        // write payload (if any)
        if (payloadBytes.Length > 0)
            Buffer.BlockCopy(payloadBytes, 0, buffer, offset, payloadBytes.Length);

        return buffer;
    }

    public RealtimeMessage<TPayload> FromBytes<TPayload>(byte[] data, MessagePackSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        if (data is null || data.Length < MessageHeader.Size)
            throw new ArgumentException("Invalid frame.", nameof(data));

        // read header (fixed)
        var header = MessageHeader.ReadFrom(data.AsSpan(0, MessageHeader.Size));
        var offset = MessageHeader.Size;

        // read telemetry if flagged
        if ((header.Flags & HeaderFlags.HasTelemetry) != 0)
        {
            if (data.Length < offset + 2) throw new InvalidOperationException("Missing telemetry length.");
            var tlvLen = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2)); offset += 2;
            if (data.Length < offset + tlvLen) throw new InvalidOperationException("Telemetry truncated.");
            var tlvSpan = data.AsSpan(offset, tlvLen);
            var telem = TelemetrySegment.ReadContentFrom(tlvSpan);
            header = header.WithTelemetry(telem);
            offset += tlvLen;
        }

        // remaining is payload (if any)
        var payloadLen = data.Length - offset;
        serializerOptions ??= _messagePackSerializerOptions;

        TPayload payload;
        if (payloadLen <= 0)
        {
            payload = CreateDefaultIfPossible<TPayload>()
                      ?? throw new InvalidOperationException("Empty binary payload for a command without parameterless ctor.");
        }
        else
        {
            var mem = new ReadOnlyMemory<byte>(data, offset, payloadLen);
            var seq = new ReadOnlySequence<byte>(mem);
            var reader = new MessagePackReader(seq);
            payload = MessagePackSerializer.Deserialize<TPayload>(ref reader, serializerOptions);
        }

        return new RealtimeMessage<TPayload>(header, payload);
    }


    // -------- Header-only --------
    public byte[] ToBytes(RealtimeMessage message)
    {
        var hasTelemetry = (message.Header.Flags & HeaderFlags.HasTelemetry) != 0 && message.Header.Telemetry.HasValue;
        var telemetryContentLen = hasTelemetry ? message.Header.Telemetry!.Value.GetContentSize() : 0;
        var telemetryTotalLen = hasTelemetry ? (2 + telemetryContentLen) : 0;

        var total = MessageHeader.Size + telemetryTotalLen;
        var buffer = new byte[total];

        message.Header.WriteTo(buffer);
        var offset = MessageHeader.Size;

        if (hasTelemetry)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), (ushort)telemetryContentLen);
            offset += 2;
            message.Header.Telemetry!.Value.WriteContentTo(buffer.AsSpan(offset, telemetryContentLen));
        }

        return buffer;
    }

    public RealtimeMessage FromBytesHeaderOnly(byte[] data)
    {
        if (data is null || data.Length < MessageHeader.Size)
            throw new ArgumentException("Invalid frame.", nameof(data));

        var header = MessageHeader.ReadFrom(data.AsSpan(0, MessageHeader.Size));
        var offset = MessageHeader.Size;

        if ((header.Flags & HeaderFlags.HasTelemetry) != 0)
        {
            if (data.Length < offset + 2) throw new InvalidOperationException("Missing telemetry length.");
            var tlvLen = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2)); offset += 2;
            if (data.Length < offset + tlvLen) throw new InvalidOperationException("Telemetry truncated.");
            var telem = TelemetrySegment.ReadContentFrom(data.AsSpan(offset, tlvLen));
            header = header.WithTelemetry(telem);
            // ignore trailing bytes if any (header-only path)
        }

        return new RealtimeMessage(header);
    }
}
