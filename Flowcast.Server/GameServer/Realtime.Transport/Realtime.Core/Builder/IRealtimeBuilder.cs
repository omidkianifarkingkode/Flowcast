using System.Reflection;

namespace Realtime.Transport.Builder;

public interface IRealtimeBuilder
{
    // chainable
    IRealtimeBuilder DiscoverMessagesFrom(params Assembly[] assemblies);
    IRealtimeBuilder UseCommandRouting(Action<ICommandRouteConfigurator> configure);
}

public interface ICommandRouteConfigurator
{
    // open-generic or non-generic markers → open-generic handlers
    ICommandRouteConfigurator Map(Type commandMarkerOpenOrNonGeneric, Type handlerOpenGeneric);
}
