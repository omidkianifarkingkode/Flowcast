using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Realtime.Transport.Gateway;
using Realtime.Transport.Http;
using Realtime.Transport.Liveness;
using Realtime.Transport.Liveness.Policies;
using Realtime.Transport.Messaging.Codec;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.Messaging.Receiver;
using Realtime.Transport.Messaging.Sender;
using Realtime.Transport.Options;
using Realtime.Transport.Routing;
using Realtime.Transport.UserConnection;
using System.Reflection;

namespace Realtime.Transport.Builder;

internal sealed class RealtimeBuilder : IRealtimeBuilder, ICommandRouteConfigurator
{
    private readonly WebApplicationBuilder appBuilder;
    private readonly IServiceCollection services;

    // we collect assemblies via a wrapper so the registry can inject IEnumerable<PayloadAssembly>
    private sealed record PayloadAssembly(Assembly Assembly);

    // routing mapping; we’ll collapse into CommandRoutingTypes if both maps are provided
    private Type? nonGenericCommandInterface;
    private Type? nonGenericHandlerOpen;
    private Type? genericCommandInterface;
    private Type? genericHandlerOpen;

    internal RealtimeBuilder(WebApplicationBuilder builder)
    {
        appBuilder = builder;
        services = builder.Services;

        // Bind options
        services.AddOptions<RealtimeOptions>()
            .BindConfiguration("Realtime")
            .ValidateDataAnnotations()
            .Validate(o => o is not null, "Realtime options missing")
            .ValidateOnStart();

        // Core gateway
        services.AddSingleton<WebSocketHandler>();
        services.AddSingleton<IUserConnectionRegistry, InMemoryUserConnectionRegistry>();

        // Sender selection by Messaging.WireFormat
        services.AddSingleton<JsonSender>();
        services.AddSingleton<BinarySender>();
        services.AddSingleton<IRealtimeMessageSender>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<RealtimeOptions>>().Value;
            return opts.Messaging.WireFormat == Messaging.Options.WireFormat.MessagePack
                ? sp.GetRequiredService<BinarySender>()
                : sp.GetRequiredService<JsonSender>();
        });

        // Receiver
        services.AddSingleton<MessageReceiver>();
        services.AddSingleton<IMessageReceiver>(sp => sp.GetRequiredService<MessageReceiver>());

        // Context + codec + factories
        services.AddScoped<IRealtimeContextAccessor, RealtimeContextAccessor>();
        services.AddSingleton<ICodec, Codec>();
        // RealtimePayloadTypeRegistry will consume IEnumerable<PayloadAssembly>
        services.AddSingleton<IRealtimePayloadTypeRegistry>(sp =>
        {
            var assemblies = sp.GetServices<PayloadAssembly>().Select(p => p.Assembly).ToArray();
            return new RealtimePayloadTypeRegistry(assemblies);
        });
        services.AddSingleton<IRealtimePayloadFactory, RealtimePayloadFactory>();
        services.AddSingleton<IMessageFactory, MessageFactory>();

        // Liveness (uses RealtimeOptions.Liveness)
        services.AddSingleton<ILivenessPolicy, ActivityTimeoutPolicy>();
        services.AddHostedService<WebSocketLivenessService>();
    }

    // ----- IRealtimeBuilder -----

    public IRealtimeBuilder DiscoverMessagesFrom(params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0) return this;
        foreach (var asm in assemblies)
        {
            services.AddSingleton(new PayloadAssembly(asm));
        }
        return this;
    }

    public IRealtimeBuilder UseCommandRouting(Action<ICommandRouteConfigurator> configure)
    {
        if (configure is null) return this;

        // collect maps
        configure(this);

        // Honor Realtime:Routing:Enabled at registration time
        var routingEnabled = appBuilder.Configuration.GetValue<bool?>("Realtime:Routing:Enabled") ?? true;
        if (!routingEnabled) return this;

        // Validate mappings; non-generic and/or generic are both allowed
        // If only one is provided, router will still work for that subset.
        var maps = new CommandRoutingTypes(
            CommandInterface: nonGenericCommandInterface!,
            CommandGenericInterface: genericCommandInterface!,
            HandlerOpenGeneric1: nonGenericHandlerOpen!,
            HandlerOpenGeneric2: genericHandlerOpen!
        );

        services.AddSingleton<IRealtimeCommandRouter>(sp => new OpenGenericCommandRouter(maps));
        services.AddHostedService<RealtimeEventRouter>();

        return this;
    }

    // ----- ICommandRouteConfigurator -----

    public ICommandRouteConfigurator Map(Type commandMarkerOpenOrNonGeneric, Type handlerOpenGeneric)
    {
        if (commandMarkerOpenOrNonGeneric is null || handlerOpenGeneric is null)
            return this;

        if (commandMarkerOpenOrNonGeneric.IsGenericTypeDefinition)
        {
            // ICommand<> mapped to ICommandHandler<,>
            genericCommandInterface = commandMarkerOpenOrNonGeneric;
            genericHandlerOpen = handlerOpenGeneric;
        }
        else
        {
            // ICommand   mapped to ICommandHandler<>
            nonGenericCommandInterface = commandMarkerOpenOrNonGeneric;
            nonGenericHandlerOpen = handlerOpenGeneric;
        }
        return this;
    }
}
