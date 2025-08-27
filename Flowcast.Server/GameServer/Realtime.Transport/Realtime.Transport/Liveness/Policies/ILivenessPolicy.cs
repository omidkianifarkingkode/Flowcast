using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Liveness.Policies;

public interface ILivenessPolicy
{
    /// <summary>Should this connection be considered for timeout checks?</summary>
    bool IsCandidateForTimeoutCheck(UserConnectionInfo c);

    /// <summary>Returns true if timed out. Provides close status/reason to use.</summary>
    bool IsTimedOut(UserConnectionInfo c, long nowUnixMs, out WebSocketCloseStatus status, out string reason);

    /// <summary>Whether an inbound frame should count as activity (kept for clarity if you later want stricter rules).</summary>
    bool CountInboundAsActivity { get; }
}