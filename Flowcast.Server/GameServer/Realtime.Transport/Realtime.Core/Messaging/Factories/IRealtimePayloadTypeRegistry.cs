using System.Collections.Concurrent;
using System.Reflection;

namespace Realtime.Transport.Messaging.Factories;

public interface IRealtimePayloadTypeRegistry
{
    Type? TryGetPayloadType(ushort type);
}

public sealed class RealtimePayloadTypeRegistry : IRealtimePayloadTypeRegistry
{
    private readonly ConcurrentDictionary<ushort, Type> _map = new();

    public RealtimePayloadTypeRegistry(params Assembly[] assembliesToScan)
    {
        if (assembliesToScan is null || assembliesToScan.Length == 0)
            assembliesToScan = new[] { Assembly.GetExecutingAssembly() };

        foreach (var asm in assembliesToScan)
            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || !typeof(IPayload).IsAssignableFrom(t)) continue;
                var attr = t.GetCustomAttribute<RealtimeMessageAttribute>();
                if (attr is null) continue;
                _map[attr.MessageType] = t;
            }
    }

    public Type? TryGetPayloadType(ushort type) =>
        _map.TryGetValue(type, out var t) ? t : null;
}