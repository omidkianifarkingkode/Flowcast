using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realtime.Transport.Gateway.Options;

public class GatewayOptions
{
    /// Size of the pooled receive buffer for WebSocket reads.
    public int ReceiveBufferBytes { get; set; } = 8192;

    /// If true, only JWT will be used to resolve userId; query/cookie fallbacks are disabled.
    public bool RequireAuthenticatedPrincipal { get; set; } = false;
}
