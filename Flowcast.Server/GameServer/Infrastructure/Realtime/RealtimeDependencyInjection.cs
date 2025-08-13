using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using Infrastructure.Realtime.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Realtime;

public static class RealtimeDependencyInjection
{
    public static WebApplicationBuilder AddRealtimeServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<WebSocketHandler>();
        builder.Services.AddSingleton<IUserConnectionRegistry, InMemoryUserConnectionRegistry>();
        builder.Services.AddSingleton<IRealtimeMessageSender, JsonRealtimeMessageSender>();
        builder.Services.AddSingleton<IRealtimeMessageReceiver, RealtimeMessageReceiver>();

        builder.Services.AddOptions<WebSocketLivenessOptions>()
            .BindConfiguration("WebSocket")
            .ValidateDataAnnotations()
            .Validate(options => options.PingIntervalSeconds > 0 && options.TimeoutSeconds > 0,
                      "PingIntervalSeconds and TimeoutSeconds must be greater than 0")
            .ValidateOnStart();

        builder.Services.AddHostedService<WebSocketLivenessService>();

        builder.Services.AddScoped<IRealtimeContextAccessor, RealtimeContextAccessor>();
        builder.Services.AddSingleton<IRealtimeCommandTypeRegistry>(new RealtimeCommandTypeRegistry(typeof(IRealtimeCommandTypeRegistry).Assembly));
        builder.Services.AddSingleton<IRealtimeCommandFactory, RealtimeCommandFactory>();
        builder.Services.AddSingleton<IRealtimeMessageCodec, RealtimeMessageCodec>();
        builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        builder.Services.AddSingleton<IHeaderMessageHandler, HeartbeatHeaderHandler>();

        return builder;
    }
}
