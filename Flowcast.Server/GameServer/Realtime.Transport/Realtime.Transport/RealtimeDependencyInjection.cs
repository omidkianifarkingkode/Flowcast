using Microsoft.AspNetCore.Builder;
using Realtime.Transport.Builder;

namespace Realtime.Transport;

public static class RealtimeDependencyInjection
{
    public static IRealtimeBuilder AddRealtimeServices(this WebApplicationBuilder builder)
        => new RealtimeBuilder(builder);

    //public static WebApplicationBuilder AddRealtimeServices(
    //    this WebApplicationBuilder builder,
    //    CommandRoutingTypes? commandRoutingTypes,
    //    params Assembly[] assembliesToScan)
    //{
    //    // Bind unified options: Realtime:Messaging / Routing / Liveness / Gateway
    //    builder.Services.AddOptions<RealtimeOptions>()
    //        .BindConfiguration("Realtime")
    //        .ValidateDataAnnotations()
    //        .Validate(o => o is not null, "Realtime options missing")
    //        .ValidateOnStart();

    //    // Core gateway
    //    builder.Services.AddSingleton<WebSocketHandler>();
    //    builder.Services.AddSingleton<IUserConnectionRegistry, InMemoryUserConnectionRegistry>();

    //    // Sender selection
    //    builder.Services.AddSingleton<JsonSender>();
    //    builder.Services.AddSingleton<BinarySender>();
    //    builder.Services.AddSingleton<IRealtimeMessageSender>(sp =>
    //    {
    //        var opts = sp.GetRequiredService<IOptions<RealtimeOptions>>().Value;
    //        return opts.Messaging.WireFormat == Messaging.Options.WireFormat.MessagePack
    //            ? sp.GetRequiredService<BinarySender>()
    //            : sp.GetRequiredService<JsonSender>();
    //    });

    //    // Receiver
    //    builder.Services.AddSingleton<IMessageReceiver, MessageReceiver>();

    //    // Liveness (reads RealtimeOptions.Liveness)
    //    builder.Services.AddSingleton<ILivenessPolicy, ActivityTimeoutPolicy>();
    //    builder.Services.AddHostedService<WebSocketLivenessService>();

    //    // Context + codec + factories
    //    builder.Services.AddScoped<IRealtimeContextAccessor, RealtimeContextAccessor>();
    //    builder.Services.AddSingleton<IRealtimePayloadTypeRegistry>(new RealtimePayloadTypeRegistry(assembliesToScan));
    //    builder.Services.AddSingleton<IRealtimePayloadFactory, RealtimePayloadFactory>();
    //    builder.Services.AddSingleton<ICodec, Codec>();
    //    builder.Services.AddSingleton<IMessageFactory, MessageFactory>();

    //    // Routing (reads RealtimeOptions.Routing)
    //    var routingEnabled = builder.Configuration.GetValue<bool?>("Realtime:Routing:Enabled") ?? true;
    //    if (routingEnabled  && commandRoutingTypes is CommandRoutingTypes routingTypes)
    //    {
    //        builder.Services.AddSingleton<IRealtimeCommandRouter>(sp =>
    //            new OpenGenericCommandRouter(routingTypes));

    //        builder.Services.AddHostedService<RealtimeEventRouter>();
    //    }

    //    return builder;
    //}
}
