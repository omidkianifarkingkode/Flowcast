using Application.Realtime.Messaging;
using Application.Realtime.Services;
using System.Collections.Concurrent;
using System.Reflection;

namespace Infrastructure.Realtime.Services;

public sealed class RealtimeCommandTypeRegistry : IRealtimeCommandTypeRegistry
{
    private readonly ConcurrentDictionary<RealtimeMessageType, Type> _map = new();

    public RealtimeCommandTypeRegistry(params Assembly[] assembliesToScan)
    {
        if (assembliesToScan is null || assembliesToScan.Length == 0)
            assembliesToScan = new[] { Assembly.GetExecutingAssembly() };

        foreach (var asm in assembliesToScan)
            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || !typeof(IRealtimeCommand).IsAssignableFrom(t)) continue;
                var attr = t.GetCustomAttribute<RealtimeMessageAttribute>();
                if (attr is null) continue;
                _map[attr.MessageType] = t;
            }
    }

    public Type? TryGetCommandType(RealtimeMessageType type) =>
        _map.TryGetValue(type, out var t) ? t : null;
}
