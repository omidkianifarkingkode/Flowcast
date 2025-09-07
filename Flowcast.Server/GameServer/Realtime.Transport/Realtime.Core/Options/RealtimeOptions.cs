using Realtime.Transport.Gateway.Options;
using Realtime.Transport.Liveness.Options;
using Realtime.Transport.Messaging.Options;
using Realtime.Transport.Routing.Options;

namespace Realtime.Transport.Options;

public class RealtimeOptions
{
    public MessagingOptions Messaging { get; set; } = new();
    public RoutingOptions Routing { get; set; } = new();
    public LivenessOptions Liveness { get; set; } = new();
    public GatewayOptions Gateway { get; set; } = new();
}
