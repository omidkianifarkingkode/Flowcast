using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Presentation.Endpoints;
using Presentation.Middleware;
using Presentation.SwaggerUtilities;
using Serilog;

namespace Presentation.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication SetupMiddlewares(this WebApplication app)
    {
        var versionedGroup = app.GetVersionedGroupBuilder();

        app.MapEndpoints(versionedGroup);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.ApplyMigrations();
        }

        app.MapHealthChecks("health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseRequestContextLogging();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();// REMARK: If you want to use Controllers, you'll need this.

        return app;
    }

    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }
}
