using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
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
        var buffer = new byte[MessageHeader.Size + payloadBytes.Length];
        message.Header.WriteTo(buffer);
        if (payloadBytes.Length > 0)
            Buffer.BlockCopy(payloadBytes, 0, buffer, MessageHeader.Size, payloadBytes.Length);
        return buffer;
    }

    public RealtimeMessage<TCommand> FromBytes<TCommand>(byte[] data, MessagePackSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
    {
        if (data is null || data.Length < MessageHeader.Size)
            throw new ArgumentException("Invalid frame.", nameof(data));

        var header = MessageHeader.ReadFrom(data.AsSpan(0, MessageHeader.Size));
        var payloadMem = new ReadOnlyMemory<byte>(data, MessageHeader.Size, data.Length - MessageHeader.Size);

        serializerOptions ??= _messagePackSerializerOptions;
        // Prefer reader to avoid API differences between versions
        var seq = new ReadOnlySequence<byte>(payloadMem);
        var reader = new MessagePackReader(seq);
        var payload = MessagePackSerializer.Deserialize<TCommand>(ref reader, serializerOptions);

        return new RealtimeMessage<TCommand>(header, payload);
    }

    // -------- Typed: JSON --------
    public string ToJson<TCommand>(RealtimeMessage<TCommand> message, JsonSerializerOptions? serializerOptions = null)
        where TCommand : IRealtimeCommand
    {
        serializerOptions ??= _jsonWriteSerializerOptions;

        var dto = new JsonWire<TCommand>
        {
            Header = new JsonWireHeader
            {
                Type = message.Header.Type,
                Id = message.Header.Id,
                Timestamp = message.Header.Timestamp
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

        if (dto.Payload is null) 
            throw new InvalidOperationException("JSON payload was null.");

        return new RealtimeMessage<TCommand>(new MessageHeader(dto.Header.Type, dto.Header.Id, dto.Header.Timestamp),
            dto.Payload ?? throw new InvalidOperationException("JSON payload was null."));
    }

    // -------- Header-only --------
    public byte[] ToBytes(RealtimeMessage message)
    {
        var buffer = new byte[MessageHeader.Size];
        message.Header.WriteTo(buffer);
        return buffer;
    }

    public RealtimeMessage FromBytesHeaderOnly(byte[] data)
    {
        if (data is null || data.Length < MessageHeader.Size)
            throw new ArgumentException("Invalid frame.", nameof(data));
        return new RealtimeMessage(MessageHeader.ReadFrom(data.AsSpan(0, MessageHeader.Size)));
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
        return JsonSerializer.Serialize(new { Header = dto }, serializerOptions);
    }

    public RealtimeMessage FromJsonHeaderOnly(string json, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var wrapper = JsonSerializer.Deserialize<Dictionary<string, JsonWireHeader>>(json, serializerOptions)
                      ?? throw new InvalidOperationException("Invalid JSON.");

        var header = wrapper
        .FirstOrDefault(kvp => string.Equals(kvp.Key, "Header", StringComparison.OrdinalIgnoreCase))
        .Value ?? throw new InvalidOperationException("Missing header.");

        return new RealtimeMessage(new MessageHeader(header.Type, header.Id, header.Timestamp));
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
    }
}
