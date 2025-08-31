using Application.Abstractions.Messaging;
using Presentation.Endpoints;
using Presentation.Extensions;
using Presentation.Infrastructure;
using Presentation.SwaggerUtilities;
using Realtime.Transport;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Presentation;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(builder.Services);

        builder.Services.InstallVersioning();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
        builder.Services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddSwaggerGen();

        builder.AddRealtimeServices()
            .DiscoverMessagesFrom(typeof(Application.DependencyInjection).Assembly)
            .UseCommandRouting(routes => routes
                .Map(typeof(ICommand), typeof(ICommandHandler<>))
                .Map(typeof(ICommand<>), typeof(ICommandHandler<,>))
        );

        return builder;
    }
}
