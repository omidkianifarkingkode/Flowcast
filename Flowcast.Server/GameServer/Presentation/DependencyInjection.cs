using Presentation.Endpoints;
using Presentation.Extensions;
using Presentation.Infrastructure;
using Presentation.SwaggerUtilities;
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

        return builder;
    }
}
