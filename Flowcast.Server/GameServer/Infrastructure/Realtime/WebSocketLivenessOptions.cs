public sealed class WebSocketLivenessOptions
{
    public int PingIntervalSeconds { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Max age for in-flight pings before purge. If <= 0, defaults to min(2*Timeout, 10*PingInterval).
    /// </summary>
    public int PendingPingTtlSeconds { get; set; } = 0;

    /// <summary>
    /// Hard cap on in-flight pings per connection. If <= 0, derived from TTL/PingInterval and clamped.
    /// </summary>
    public int MaxPendingPingsPerConnection { get; set; } = 0;

    /// <summary>
    /// If true, include last-known telemetry (RTT/LastPingId) in the Ping header.
    /// If no telemetry exists yet, a default (0,0,0) segment is sent.
    /// </summary>
    public bool IncludeTelemetryInPing { get; set; } = true;

    // <summary>
    /// If true, any client message (binary or text) counts as heartbeat.
    /// If false, liveness requires a Pong (or we time out).
    /// </summary>
    public bool TreatAnyClientMessageAsHeartbeat { get; set; } = true;

    /// <summary>
    /// If true, only send a Ping when the connection was idle for at least PingInterval.
    /// If false, always send a Ping each tick regardless of activity.
    /// </summary>
    public bool AdaptivePing { get; set; } = true;

    public TimeSpan GetPingInterval() => TimeSpan.FromSeconds(PingIntervalSeconds);
    public TimeSpan GetTimeout() => TimeSpan.FromSeconds(TimeoutSeconds);

    public TimeSpan GetPendingPingTtl()
    {
        if (PendingPingTtlSeconds > 0) return TimeSpan.FromSeconds(PendingPingTtlSeconds);
        var derived = Math.Min(TimeoutSeconds * 2, PingIntervalSeconds * 10);
        return TimeSpan.FromSeconds(Math.Max(derived, PingIntervalSeconds * 2)); // ensure TTL >= 2 intervals
    }

    public int GetMaxPendingPings()
    {
        if (MaxPendingPingsPerConnection > 0) return MaxPendingPingsPerConnection;

        var ttl = GetPendingPingTtl().TotalSeconds;
        var pi = Math.Max(1, PingIntervalSeconds);
        var derived = (int)Math.Ceiling(ttl / pi) + 2; // small buffer
        // clamp into a small, safe range
        return Math.Clamp(derived, 8, 32);
    }
}
