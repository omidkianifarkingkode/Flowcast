using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Presentation.Endpoints;
using Realtime.Transport.Gateway;
using Shared.API.Swagger;
using Shared.API.Loggings;

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

        app.UseMiddleware<RequestContextLoggingMiddleware>();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();// REMARK: If you want to use Controllers, you'll need this.

        app.UseRealtime();

        return app;
    }
}
