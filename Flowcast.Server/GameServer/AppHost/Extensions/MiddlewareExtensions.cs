using Identity.Presentation;
using Realtime.Transport.Gateway;
using Serilog;
using Shared.Infrastructure.Database;
using Shared.Presentation.Endpoints;
using Shared.Presentation.Swagger;
using Shared.Infrastructure.Extensions;

namespace AppHost.Extensions;

public static class MiddlewareExtensions
{
    public static async Task<WebApplication> UseAppHost(this WebApplication app)
    {
        // 1) Error handling as early as possible
        app.UseExceptionHandler();

        //app.UseMiddleware<RequestContextLoggingMiddleware>();

        // 2) HTTPS / HSTS (optional but recommended)
        if (!app.Environment.IsLocalOrDevelopement())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        // 3) Swagger (dev only)
        if (app.Environment.IsLocalOrDevelopement())
        {
            await app.ApplyAllMigrationsAsync();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 4) Routing (explicit is clearer if you also map controllers)
        app.UseRouting();

        // 5) Request logging AFTER routing to capture route template
        app.UseSerilogRequestLogging();

        // 7) Auth
        await app.UseIdentity();

        // 8) Map endpoints
        app.MapEndpoints();

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
