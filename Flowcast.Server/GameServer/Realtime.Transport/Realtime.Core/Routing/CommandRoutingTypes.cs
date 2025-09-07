namespace Realtime.Transport.Routing;

/// <summary>
/// Open-generic type handles supplied at composition time (no compile-time dependency on your Application project).
/// </summary>
public readonly record struct CommandRoutingTypes(
    Type CommandInterface,            // e.g., typeof(Application.Abstractions.Messaging.ICommand)
    Type CommandGenericInterface,     // e.g., typeof(Application.Abstractions.Messaging.ICommand<>)
    Type HandlerOpenGeneric1,         // e.g., typeof(Application.Abstractions.Messaging.ICommandHandler<>)
    Type HandlerOpenGeneric2          // e.g., typeof(Application.Abstractions.Messaging.ICommandHandler<,>)
);
