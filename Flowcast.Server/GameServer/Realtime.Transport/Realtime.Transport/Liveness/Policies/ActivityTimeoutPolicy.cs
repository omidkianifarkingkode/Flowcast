using Microsoft.Extensions.Options;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Liveness.Policies;

public sealed class ActivityTimeoutPolicy(IOptions<WebSocketLivenessOptions> options) : ILivenessPolicy
{
    public bool CountInboundAsActivity => options.Value.CountAnyInboundAsActivity;

    public bool IsCandidateForTimeoutCheck(UserConnectionInfo c)
        => c.IsConnected && c.Socket.State == WebSocketState.Open;

    public bool IsTimedOut(UserConnectionInfo userConnectionInfo, long nowUnixMs, out WebSocketCloseStatus status, out string reason)
    {
        var idleMs = nowUnixMs - userConnectionInfo.LastClientActivity;
        var timeoutMs = options.Value.GetTimeout().TotalMilliseconds;

        if (idleMs > timeoutMs)
        {
            status = options.Value.CloseStatusOnTimeout;
            reason = options.Value.CloseReasonOnTimeout;
            return true;
        }

        status = default;
        reason = string.Empty;
        return false;
    }
}
