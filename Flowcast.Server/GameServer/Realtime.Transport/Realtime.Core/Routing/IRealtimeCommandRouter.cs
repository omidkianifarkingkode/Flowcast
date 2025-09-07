using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Realtime.Transport.Http;
using Realtime.Transport.Messaging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Realtime.Transport.Routing;

public interface IRealtimeCommandRouter
{
    bool CanRoute(IPayload payload);
    Task RouteAsync(IServiceProvider serviceProvider, RealtimeContext context, IPayload payload, CancellationToken cancellationToken);
}

public sealed class OpenGenericCommandRouter(CommandRoutingTypes commandRoutingTypes) : IRealtimeCommandRouter
{
    private readonly ConcurrentDictionary<Type, Func<IServiceProvider, IPayload, CancellationToken, Task>> invokerCache = new();

    public bool CanRoute(IPayload payload)
    {
        var payloadType = payload.GetType();
        if (commandRoutingTypes.CommandInterface.IsAssignableFrom(payloadType))
            return true;

        return payloadType
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandRoutingTypes.CommandGenericInterface);
    }

    public Task RouteAsync(IServiceProvider serviceProvider, RealtimeContext context, IPayload payload, CancellationToken cancellationToken)
    {
        var payloadType = payload.GetType();
        var invoker = invokerCache.GetOrAdd(payloadType, BuildInvoker);
        return invoker(serviceProvider, payload, cancellationToken);
    }

    private Func<IServiceProvider, IPayload, CancellationToken, Task> BuildInvoker(Type payloadType)
    {
        // Prefer ICommand<TResponse>
        var genericCommandInterface = payloadType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandRoutingTypes.CommandGenericInterface);

        if (genericCommandInterface is not null)
        {
            var responseType = genericCommandInterface.GetGenericArguments()[0];
            var handlerType = commandRoutingTypes.HandlerOpenGeneric2.MakeGenericType(payloadType, responseType);
            var handleMethod = handlerType.GetMethod("Handle")!; // Task<Result<T>> Handle(T payload, CancellationToken)
            return Compile(handlerType, handleMethod, payloadType);
        }

        // Fallback to ICommand
        if (commandRoutingTypes.CommandInterface.IsAssignableFrom(payloadType))
        {
            var handlerType = commandRoutingTypes.HandlerOpenGeneric1.MakeGenericType(payloadType);
            var handleMethod = handlerType.GetMethod("Handle")!; // Task<Result> Handle(T payload, CancellationToken)
            return Compile(handlerType, handleMethod, payloadType);
        }

        // No-op for non-commands
        return static (_, _, _) => Task.CompletedTask;
    }

    private static Func<IServiceProvider, IPayload, CancellationToken, Task> Compile(Type handlerType, MethodInfo handleMethod, Type payloadType)
    {
        // (IServiceProvider serviceProvider, IPayload payload, CancellationToken cancellationToken) =>
        //     serviceProvider.GetRequiredService(handlerType).Handle((TPayload)payload, cancellationToken)
        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var payloadParameter = Expression.Parameter(typeof(IPayload), "payload");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var getRequiredServiceGeneric = typeof(ServiceProviderServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(handlerType);

        var resolveHandlerExpression = Expression.Call(getRequiredServiceGeneric, serviceProviderParameter);
        var castPayloadExpression = Expression.Convert(payloadParameter, payloadType);
        var callHandleExpression = Expression.Call(resolveHandlerExpression, handleMethod, castPayloadExpression, cancellationTokenParameter);

        var asTaskExpression = Expression.Convert(callHandleExpression, typeof(Task));
        var lambda = Expression.Lambda<Func<IServiceProvider, IPayload, CancellationToken, Task>>(
            asTaskExpression, serviceProviderParameter, payloadParameter, cancellationTokenParameter);

        return lambda.Compile();
    }
}