using Application.Realtime.Messaging;
using Application.Realtime.Services;
using MessagePack;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Realtime.Services;

public sealed class RealtimeMessageCodec : IRealtimeMessageCodec
{
    private readonly JsonSerializerOptions _jsonWriteSerializerOptions;
    private readonly JsonSerializerOptions _jsonReadSerializerOptions;
    private readonly MessagePackSerializerOptions _messagePackSerializerOptions;

    public RealtimeMessageCodec(
        JsonSerializerOptions? jsonWriteSerializerOptions = null,
        JsonSerializerOptions? jsonReadSerializerOptions = null,
        MessagePackSerializerOptions? messagePackSerializerOptions = null)
    {
        _jsonWriteSerializerOptions = jsonWriteSerializerOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        _jsonReadSerializerOptions = jsonReadSerializerOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        _messagePackSerializerOptions = messagePackSerializerOptions ?? MessagePackSerializerOptions.Standard;
    }

    // -------- Typed: Binary --------
    public byte[] ToBytes<TCommand>(RealtimeMessage<TCommand> message, MessagePackSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
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

    public RealtimeMessage<TCommand> FromBytes<TCommand>(byte[] data, MessagePackSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
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

        TCommand payload;
        if (payloadLen <= 0)
        {
            payload = CreateDefaultIfPossible<TCommand>()
                      ?? throw new InvalidOperationException("Empty binary payload for a command without parameterless ctor.");
        }
        else
        {
            var payloadMem = new ReadOnlyMemory<byte>(data, offset, payloadLen);
            var seq = new ReadOnlySequence<byte>(payloadMem);
            var reader = new MessagePackReader(seq);
            payload = MessagePackSerializer.Deserialize<TCommand>(ref reader, serializerOptions);
        }

        return new RealtimeMessage<TCommand>(header, payload);
    }

    // -------- Typed: JSON --------
    public string ToJson<TCommand>(RealtimeMessage<TCommand> message, JsonSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
    {
        serializerOptions ??= _jsonWriteSerializerOptions;

        TelemetryDto? telem = null;
        if ((message.Header.Flags & HeaderFlags.HasTelemetry) != 0 && message.Header.Telemetry is TelemetrySegment ts)
            telem = new TelemetryDto { LastPingId = ts.LastPingId, LastRttMs = ts.LastRttMs, ClientSendTs = ts.ClientSendTs };


        var dto = new JsonWire<TCommand>
        {
            Header = new JsonWireHeader
            {
                Type = message.Header.Type,
                Id = message.Header.Id,
                Timestamp = message.Header.Timestamp,
                Flags = message.Header.Flags,
                Telemetry = telem
            },
            Payload = message.Payload
        };

        return JsonSerializer.Serialize(dto, serializerOptions);
    }

    public RealtimeMessage<TCommand> FromJson<TCommand>(string json, JsonSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var dto = JsonSerializer.Deserialize<JsonWire<TCommand>>(json, serializerOptions)
                  ?? throw new InvalidOperationException("Invalid JSON.");

        // If payload is missing (header-only on the wire), synthesize an instance for empty commands
        var payload = dto.Payload ?? CreateDefaultIfPossible<TCommand>();

        if (payload is null)
            throw new InvalidOperationException("JSON payload was null and command is not parameterless.");

        return new RealtimeMessage<TCommand>(new MessageHeader(dto.Header.Type, dto.Header.Id, dto.Header.Timestamp),
            payload);
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

    public string ToJson(RealtimeMessage message, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= _jsonWriteSerializerOptions;
        var dto = new JsonWireHeader
        {
            Type = message.Header.Type,
            Id = message.Header.Id,
            Timestamp = message.Header.Timestamp
        };
        return JsonSerializer.Serialize(new { Header = dto, Payload = (object?)null }, serializerOptions);
    }

    public RealtimeMessage FromJsonHeaderOnly(string json, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var typed = JsonSerializer.Deserialize<HeaderOnlyWrapper>(json, serializerOptions)
                    ?? throw new InvalidOperationException("Invalid JSON.");

        if (typed.Header is null) throw new InvalidOperationException("Missing header.");

        var flags = typed.Header.Flags;
        TelemetrySegment? telem = null;

        if ((flags & HeaderFlags.HasTelemetry) != 0 && typed.Header.Telemetry is TelemetryDto t)
            telem = new TelemetrySegment(t.LastPingId, t.LastRttMs, t.ClientSendTs);

        return new RealtimeMessage(new MessageHeader(typed.Header.Type, typed.Header.Id, typed.Header.Timestamp, flags, telem));
    }

    private static T? CreateDefaultIfPossible<T>() where T : IRealtimeCommand
    {
        var t = typeof(T);
        // Allow parameterless construction for empty commands like PongCommand/PingCommand
        return (T?)Activator.CreateInstance(t);
    }

    private sealed class JsonWire<T> where T : IRealtimeCommand
    {
        public JsonWireHeader Header { get; set; } = default!;
        public T? Payload { get; set; }
    }

    private sealed class JsonWireHeader
    {
        [JsonConverter(typeof(JsonStringEnumConverter))] // forces enum as string
        public RealtimeMessageType Type { get; set; }
        public ulong Id { get; set; }
        public long Timestamp { get; set; }
        public HeaderFlags Flags { get; set; } = HeaderFlags.None;
        public TelemetryDto? Telemetry { get; set; }
    }

    private sealed class TelemetryDto
    {
        public ulong LastPingId { get; set; }
        public int LastRttMs { get; set; }
        public long ClientSendTs { get; set; }
    }

    private sealed class HeaderOnlyWrapper
    {
        public JsonWireHeader? Header { get; set; }
        public object? Payload { get; set; } // ignored for header-only
    }
}
