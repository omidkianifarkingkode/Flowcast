using HealthChecks.UI.Client;
using Identity.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Realtime.Transport.Gateway;
using Serilog;
using Shared.API.Endpoints;
using Shared.API.Loggings;
using Shared.API.Swagger;
using Shared.API.Versioning;
using System.Threading.Tasks;

namespace Presentation.Extensions;

public static class MiddlewareExtensions
{
    public static async Task<WebApplication> UseAppHost(this WebApplication app)
    {
        // 1) Error handling as early as possible
        app.UseExceptionHandler();

        //app.UseMiddleware<RequestContextLoggingMiddleware>();

        // 2) HTTPS / HSTS (optional but recommended)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        // 3) Swagger (dev only)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 4) Routing (explicit is clearer if you also map controllers)
        app.UseRouting();

        // 5) Request logging AFTER routing to capture route template
        app.UseSerilogRequestLogging();

        // 7) Auth
        await app.UseIdentity(indentity => 
        {
            indentity.UseAuthorization = true;
        });

        // 8) Map endpoints
        var versionedGroup = app.GetVersionedGroupBuilder();
        app.MapEndpoints(versionedGroup);

        // Controllers (if you use MVC controllers)
        app.MapControllers();

        // Health checks (often unauthenticated)
        //app.MapHealthChecks("/health", new HealthCheckOptions
        //{
        //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        //});

        // 9) Realtime (place after auth if it needs identity; ensure it maps endpoints/sockets here)
        app.UseRealtime();

        return app;
    }
}
