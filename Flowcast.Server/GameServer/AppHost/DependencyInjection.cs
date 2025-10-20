using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using Serilog;
using Shared.Infrastructure;
using Shared.Presentation.Swagger;
using Shared.Presentation.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Presentation;

public static class DependencyInjection
{
    internal static WebApplicationBuilder ConfigureBuildingBlocks(this WebApplicationBuilder builder) 
    {
        builder.AddInfrastructure();

        return builder;
    }

    public static WebApplicationBuilder ConfigureAppHost(this WebApplicationBuilder builder)
    {
        // Logging
        builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

        // Versioning (make sure ApiExplorer integration is on, for Swagger)
        builder.Services.InstallVersioning();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.Configure<SwaggerGenOptions>(o =>
        {
            // e.g., add operation/schema filters for Identity endpoints if you have any
            // _.OperationFilter<IdentityAuthHeaderFilter>();
            o.CustomSchemaIds(t => t.FullName!.Replace('+', '-'));
        });

        // Controllers & JSON
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                var j = opts.JsonSerializerOptions;
                j.ReferenceHandler = ReferenceHandler.IgnoreCycles;          // optional; prefer DTOs if possible
                j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                j.Converters.Add(new JsonStringEnumConverter());             // enums as strings
                j.ReadCommentHandling = JsonCommentHandling.Skip;
                j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        // API behavior (optional, if you want custom validation responses)
        builder.Services.Configure<ApiBehaviorOptions>(o =>
        {
            o.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddSingleton(builder.Services);

        // ProblemDetails + exception handler
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddMemoryCache();

        //builder.AddRealtimeServices()
        //    .DiscoverMessagesFrom(typeof(Application.DependencyInjection).Assembly)
        //    .UseCommandRouting(routes => routes
        //        .Map(typeof(ICommand), typeof(ICommandHandler<>))
        //        .Map(typeof(ICommand<>), typeof(ICommandHandler<,>))
        //);

        return builder;
    }
}
