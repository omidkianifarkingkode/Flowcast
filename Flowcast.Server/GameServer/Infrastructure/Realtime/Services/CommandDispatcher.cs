using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Infrastructure.Realtime.Services;

public sealed class CommandDispatcher(IServiceScopeFactory scopes) : ICommandDispatcher
{
    public async Task<Result> DispatchAsync(Guid userId, IRealtimeMessage message, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var sp = scope.ServiceProvider;

        // Put header/user into scoped context if available
        var accessor = sp.GetService<IRealtimeContextAccessor>();
        if (accessor is not null)
            accessor.Current = new RealtimeContext { UserId = userId, Header = message.Header };

        try
        {
            // If no payload (Ping/Pong), handle specially or no-op
            if (message is not IRealtimePayloadMessage withPayload)
            {
                var headerHandlers = sp.GetServices<IHeaderMessageHandler>();
                foreach (var headerHandler in headerHandlers)
                {
                    if (await headerHandler.TryHandleAsync(userId, (RealtimeMessage)message, ct))
                        return Result.Success();
                }

                return Result.Success();
            }

            var cmd = (ICommand)withPayload.Payload;
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(cmd.GetType());
            var handler = sp.GetRequiredService(handlerType);

            var invoker = InvokerCache.Get(cmd.GetType());
            return await invoker(handler, cmd, ct);
        }
        finally
        {
            if (accessor is not null) accessor.Current = null;
        }
    }

    private static class InvokerCache
    {
        private static readonly ConcurrentDictionary<Type, Func<object, ICommand, CancellationToken, Task<Result>>> _cache = new();

        public static Func<object, ICommand, CancellationToken, Task<Result>> Get(Type cmdType)
            => _cache.GetOrAdd(cmdType, Build);

        private static Func<object, ICommand, CancellationToken, Task<Result>> Build(Type cmdType)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(cmdType);

            var h = Expression.Parameter(typeof(object), "handler");
            var c = Expression.Parameter(typeof(ICommand), "command");
            var ct = Expression.Parameter(typeof(CancellationToken), "ct");

            var castH = Expression.Convert(h, handlerType);
            var castC = Expression.Convert(c, cmdType);
            var call = Expression.Call(castH, handlerType.GetMethod("Handle")!, castC, ct);

            return Expression.Lambda<Func<object, ICommand, CancellationToken, Task<Result>>>(call, h, c, ct).Compile();
        }
    }
}
