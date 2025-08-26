using System.Text.Json;

namespace Realtime.Transport.Messaging.Codec;

public sealed partial class Codec
{
    public string ToJson<TPayload>(RealtimeMessage<TPayload> message, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        serializerOptions ??= _jsonWriteSerializerOptions;

        TelemetryDto? telem = null;
        if ((message.Header.Flags & HeaderFlags.HasTelemetry) != 0 && message.Header.Telemetry is TelemetrySegment ts)
            telem = new TelemetryDto { LastPingId = ts.LastPingId, LastRttMs = ts.LastRttMs, ClientSendTs = ts.ClientSendTs };


        var dto = new JsonWire<TPayload>
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

    public RealtimeMessage<TPayload> FromJson<TPayload>(string json, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var dto = JsonSerializer.Deserialize<JsonWire<TPayload>>(json, serializerOptions)
                  ?? throw new InvalidOperationException("Invalid JSON.");

        // If payload is missing (header-only on the wire), synthesize an instance for empty commands
        var payload = dto.Payload ?? CreateDefaultIfPossible<TPayload>();

        if (payload is null)
            throw new InvalidOperationException("JSON payload was null and command is not parameterless.");

        return new RealtimeMessage<TPayload>(new MessageHeader(dto.Header.Type, dto.Header.Id, dto.Header.Timestamp),
            payload);
    }


    // -------- Header-only --------

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
}
