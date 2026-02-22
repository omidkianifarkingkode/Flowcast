using AppHost.Extensions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using Serilog;
using Shared.Infrastructure;
using Shared.Presentation;
using Shared.Presentation.ApiGuard;
using Shared.Presentation.Swagger;
using Shared.Presentation.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppHost;

public static class DependencyInjection
{
    public static WebApplicationBuilder ConfigureBuildingBlocks(this WebApplicationBuilder builder) 
    {
        builder.AddPresentation();
        builder.AddInfrastructure();

        //builder.AddRealtimeServices()
        //    .DiscoverMessagesFrom(typeof(Application.DependencyInjection).Assembly)
        //    .UseCommandRouting(routes => routes
        //        .Map(typeof(ICommand), typeof(ICommandHandler<>))
        //        .Map(typeof(ICommand<>), typeof(ICommandHandler<,>))
        //);

        return builder;
    }

    public static WebApplicationBuilder ConfigureAppHost(this WebApplicationBuilder builder)
    {
        builder.ConfigureAppSettings();
        builder.Services.AddApiGuard(builder.Configuration);
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (origins is { Length: > 0 })
                p.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            else
                p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }));

        return builder;
    }
}
