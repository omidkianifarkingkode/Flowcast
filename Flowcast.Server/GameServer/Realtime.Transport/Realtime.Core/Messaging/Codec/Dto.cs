namespace Realtime.Transport.Messaging.Codec;

public sealed partial class Codec
{
    private sealed class JsonWire<T> where T : IPayload
    {
        public JsonWireHeader Header { get; set; } = default!;
        public T? Payload { get; set; }
    }

    private sealed class JsonWireHeader
    {
        public ushort Type { get; set; }
        public ulong Id { get; set; }
        public long Timestamp { get; set; }
        public HeaderFlags Flags { get; set; } = HeaderFlags.None;
        public TelemetryDto? Telemetry { get; set; }
    }

    private sealed class TelemetryDto
    {
        public Dictionary<ushort, string>? Fields { get; set; } // key -> base64(value)
    }

    private sealed class HeaderOnlyWrapper
    {
        public JsonWireHeader? Header { get; set; }
        public object? Payload { get; set; } // ignored for header-only
    }
}
