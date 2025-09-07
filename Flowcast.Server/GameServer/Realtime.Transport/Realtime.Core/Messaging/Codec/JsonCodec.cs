using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace Realtime.Transport.Messaging.Codec;

public sealed partial class Codec
{
    // -------- Typed payload --------

    public byte[] ToJson<TPayload>(RealtimeMessage<TPayload> message, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        serializerOptions ??= _jsonWriteSerializerOptions;

        TelemetryDto? telem = null;
        if ((message.Header.Flags & HeaderFlags.HasTelemetry) != 0 && message.Header.Telemetry.HasValue)
        {
            telem = new TelemetryDto
            {
                // Represent TLVs as key -> base64(value)
                Fields = message.Header.Telemetry.Value.Fields.ToDictionary(
                    f => f.Key,
                    f => Convert.ToBase64String(f.Value))
            };
        }

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

        return JsonSerializer.SerializeToUtf8Bytes(dto, serializerOptions);
    }

    public RealtimeMessage<TPayload> FromJson<TPayload>(string json, JsonSerializerOptions? serializerOptions = null)
        where TPayload : IPayload
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var dto = JsonSerializer.Deserialize<JsonWire<TPayload>>(json, serializerOptions)
                  ?? throw new InvalidOperationException("Invalid JSON.");

        var flags = dto.Header.Flags;

        // Rebuild TLV telemetry from DTO if present
        TelemetrySegment? telem = null;
        if ((flags & HeaderFlags.HasTelemetry) != 0 && dto.Header.Telemetry?.Fields is { Count: > 0 } fields)
        {
            var list = new List<TelemetryField>(fields.Count);
            foreach (var kv in fields)
            {
                var bytes = string.IsNullOrEmpty(kv.Value) ? Array.Empty<byte>() : Convert.FromBase64String(kv.Value);
                list.Add(new TelemetryField(kv.Key, bytes));
            }
            telem = new TelemetrySegment(list);
        }

        var payload = dto.Payload ?? CreateDefaultIfPossible<TPayload>()
                      ?? throw new InvalidOperationException("JSON payload was null and command is not parameterless.");

        var header = new MessageHeader(dto.Header.Type, dto.Header.Id, dto.Header.Timestamp, flags, telem);
        return new RealtimeMessage<TPayload>(header, payload);
    }

    // -------- Header-only --------

    public byte[] ToJson(RealtimeMessage message, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= _jsonWriteSerializerOptions;

        TelemetryDto? telem = null;
        if ((message.Header.Flags & HeaderFlags.HasTelemetry) != 0 && message.Header.Telemetry.HasValue)
        {
            telem = new TelemetryDto
            {
                Fields = message.Header.Telemetry.Value.Fields.ToDictionary(
                    f => f.Key,
                    f => Convert.ToBase64String(f.Value))
            };
        }

        var dto = new
        {
            Header = new JsonWireHeader
            {
                Type = message.Header.Type,
                Id = message.Header.Id,
                Timestamp = message.Header.Timestamp,
                Flags = message.Header.Flags,
                Telemetry = telem
            },
            Payload = (object?)null
        };

        return JsonSerializer.SerializeToUtf8Bytes(dto, serializerOptions);
    }

    public RealtimeMessage FromJsonHeaderOnly(string json, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= _jsonReadSerializerOptions;

        var typed = JsonSerializer.Deserialize<HeaderOnlyWrapper>(json, serializerOptions)
                    ?? throw new InvalidOperationException("Invalid JSON.");
        if (typed.Header is null) throw new InvalidOperationException("Missing header.");

        var flags = typed.Header.Flags;

        TelemetrySegment? telem = null;
        if ((flags & HeaderFlags.HasTelemetry) != 0 && typed.Header.Telemetry?.Fields is { Count: > 0 } fields)
        {
            var list = new List<TelemetryField>(fields.Count);
            foreach (var kv in fields)
            {
                var bytes = string.IsNullOrEmpty(kv.Value) ? Array.Empty<byte>() : Convert.FromBase64String(kv.Value);
                list.Add(new TelemetryField(kv.Key, bytes));
            }
            telem = new TelemetrySegment(list);
        }

        var header = new MessageHeader(typed.Header.Type, typed.Header.Id, typed.Header.Timestamp, flags, telem);
        return new RealtimeMessage(header);
    }
}
