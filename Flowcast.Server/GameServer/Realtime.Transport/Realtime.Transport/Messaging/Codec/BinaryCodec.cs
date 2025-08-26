using MessagePack;
using System.Buffers;

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

        var total = MessageHeader.Size + (hasTelemetry ? TelemetrySegment.Size : 0) + payloadBytes.Length;

        var buffer = new byte[total];

        message.Header.WriteTo(buffer);

        var offset = MessageHeader.Size;

        // write telemetry (optional, immediately after header)
        if (hasTelemetry)
        {
            message.Header.Telemetry!.Value.WriteTo(buffer.AsSpan(offset, TelemetrySegment.Size));
            offset += TelemetrySegment.Size;
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
            if (data.Length < offset + TelemetrySegment.Size)
                throw new InvalidOperationException("Frame flagged with HasTelemetry but segment is incomplete.");

            var telem = TelemetrySegment.ReadFrom(new ReadOnlySpan<byte>(data, offset, TelemetrySegment.Size));
            header = header.WithTelemetry(telem);
            offset += TelemetrySegment.Size;
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
            var payloadMem = new ReadOnlyMemory<byte>(data, offset, payloadLen);
            var seq = new ReadOnlySequence<byte>(payloadMem);
            var reader = new MessagePackReader(seq);
            payload = MessagePackSerializer.Deserialize<TPayload>(ref reader, serializerOptions);
        }

        return new RealtimeMessage<TPayload>(header, payload);
    }


    // -------- Header-only --------
    public byte[] ToBytes(RealtimeMessage message)
    {
        var hasTelemetry = (message.Header.Flags & HeaderFlags.HasTelemetry) != 0
                           && message.Header.Telemetry.HasValue;

        var total = MessageHeader.Size + (hasTelemetry ? TelemetrySegment.Size : 0);
        var buffer = new byte[total];

        message.Header.WriteTo(buffer);

        if (hasTelemetry)
            message.Header.Telemetry!.Value.WriteTo(buffer.AsSpan(MessageHeader.Size, TelemetrySegment.Size));

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
            if (data.Length < offset + TelemetrySegment.Size)
                throw new InvalidOperationException("Frame flagged with HasTelemetry but segment is incomplete.");

            var telem = TelemetrySegment.ReadFrom(new ReadOnlySpan<byte>(data, offset, TelemetrySegment.Size));
            header = header.WithTelemetry(telem);
            // no payload in header-only path, but OK if extra bytes are present (ignore)
        }

        return new RealtimeMessage(header);
    }
}
