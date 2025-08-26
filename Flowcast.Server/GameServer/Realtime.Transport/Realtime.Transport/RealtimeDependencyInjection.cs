using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Realtime.Transport.Gateway;
using Realtime.Transport.Http;
using Realtime.Transport.Liveness;
using Realtime.Transport.Messaging.Codec;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.Messaging.Receiver;
using Realtime.Transport.Messaging.Sender;
using Realtime.Transport.UserConnection;
using System.Reflection;

namespace Realtime.Transport;

public static class RealtimeDependencyInjection
{
    public static WebApplicationBuilder AddRealtimeServices(this WebApplicationBuilder builder, params Assembly[] assembliesToScan)
    {
        builder.Services.AddSingleton<WebSocketHandler>();
        builder.Services.AddSingleton<IUserConnectionRegistry, InMemoryUserConnectionRegistry>();
        builder.Services.AddSingleton<IRealtimeMessageSender, JsonSender>();
        builder.Services.AddSingleton<RealtimeMessageReceiver>();
        builder.Services.AddSingleton<IRealtimeMessageReceiver>(sp => sp.GetRequiredService<RealtimeMessageReceiver>());
        builder.Services.AddSingleton<IRealtimeGateway>(sp => sp.GetRequiredService<RealtimeMessageReceiver>());

        builder.Services.AddOptions<WebSocketLivenessOptions>()
            .BindConfiguration("WebSocket")
            .ValidateDataAnnotations()
            .Validate(options => options.PingIntervalSeconds > 0 && options.TimeoutSeconds > 0,
                      "PingIntervalSeconds and TimeoutSeconds must be greater than 0")
            .ValidateOnStart();

        builder.Services.AddHostedService<WebSocketLivenessService>();

        builder.Services.AddScoped<IRealtimeContextAccessor, RealtimeContextAccessor>();
        builder.Services.AddSingleton<IRealtimePayloadTypeRegistry>(new RealtimePayloadTypeRegistry(assembliesToScan));
        builder.Services.AddSingleton<IRealtimePayloadFactory, RealtimePayloadFactory>();
        builder.Services.AddSingleton<ICodec, Codec>();
        builder.Services.AddSingleton<IMessageFactory, MessageFactory>();

        return builder;
    }
}
