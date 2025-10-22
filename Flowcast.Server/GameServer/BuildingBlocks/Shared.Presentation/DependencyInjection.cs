using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shared.Presentation.ErrorHandling;
using Shared.Presentation.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Presentation.Swagger;

namespace Shared.Presentation;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.InstallVersioning();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddSingleton(builder.Services);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Controllers & JSON
        builder.Services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                var j = opts.JsonSerializerOptions;
                j.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                j.Converters.Add(new JsonStringEnumConverter());
                j.ReadCommentHandling = JsonCommentHandling.Skip;
                j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        // API behavior (optional, if you want custom validation responses)
        builder.Services.Configure<ApiBehaviorOptions>(o =>
        {
            o.SuppressModelStateInvalidFilter = true;
        });

        return builder;
    }
}
