using System.Net.WebSockets;

namespace Realtime.Transport.Liveness;

public sealed class WebSocketLivenessOptions
{
    /// <summary>Seconds of no inbound activity before the server closes the connection.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Close status used when a connection times out.</summary>
    public WebSocketCloseStatus CloseStatusOnTimeout { get; set; } = WebSocketCloseStatus.NormalClosure;

    /// <summary>UTF-8 reason sent with the close frame.</summary>
    public string CloseReasonOnTimeout { get; set; } = "idle-timeout";

    /// <summary>If true, ANY inbound frame counts as activity (recommended for your plan).</summary>
    public bool CountAnyInboundAsActivity { get; set; } = true;

    public TimeSpan GetTimeout() => TimeSpan.FromSeconds(Math.Max(1, TimeoutSeconds));
}
