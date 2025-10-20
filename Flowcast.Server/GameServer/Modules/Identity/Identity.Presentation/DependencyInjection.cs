using Identity.Application;
using Identity.Application.Services;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Presentation.Endpoints;

namespace Identity.Presentation;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpoints(typeof(DependencyInjection).Assembly);

        builder.AddApplication();
        builder.AddInfrastructure();

        return builder;
    }

    public static async Task UseIdentity(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        await SeedDatabase(app);
    }

    // -------------------- private helpers --------------------

    private static async Task SeedDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
        var keyStore = scope.ServiceProvider.GetRequiredService<IKeyStore>();

        var active = await keyStore.GetActiveAsync(CancellationToken.None);
        if (active is null)
        {
            logger.LogInformation("No active key found. Rotating…");
            var km = await keyStore.RotateAsync(null, null, null, CancellationToken.None);
            logger.LogInformation("Rotated. New KID: {Kid}", km.Kid);
        }
        else
        {
            logger.LogInformation("Active key present: {Kid}", active.Kid);
        }
    }
}
