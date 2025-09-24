using Asp.Versioning;
using Identity.API.Endpoints;
using Identity.API.Persistence;
using Identity.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.API.Swagger;

namespace Identity.API.Extensions;

public static class UseModuleExtension 
{
    public sealed class IdentityRuntimeOptions
    {
        public bool MigrateOnStartup { get; set; } = true;
        public bool SeedOnStartup { get; set; } = true;
        public bool ContributeSwagger { get; set; } = false; // add filters only
        public bool UseAuthorization { get; set; } = false;
    }

    public static async Task UseIdentity(this WebApplication app, Action<IdentityRuntimeOptions>? configure = null)
    {
        var setupOptions = new IdentityRuntimeOptions();
        configure?.Invoke(setupOptions);

        await SetupDevelopment(app, setupOptions);

        if (setupOptions.UseAuthorization)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        MapEndpoints(app);

        if (setupOptions.SeedOnStartup)
            await SeedDatabase(app);
    }


    // -------------------- private helpers --------------------

    private static async Task SetupDevelopment(WebApplication app, IdentityRuntimeOptions setupOptions)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();

            if (setupOptions.MigrateOnStartup)
            {
                var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                await db.Database.MigrateAsync();
            }

            if (setupOptions.ContributeSwagger)
                app.UseSwagger();
        }
    }

    private static void MapEndpoints(WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var v1 = app.MapGroup("")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

        v1.MapDeviceSignInEndpoint();
        v1.MapGoogleSignInEndpoint();
        v1.MapGetProfileEndpoint();
        v1.MapJwksEndpoint();
        v1.MapLinkEndpoint();
        v1.MapLogoutEndpoint();
        v1.MapRefreshEndpoint();
    }

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
