using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Extensions;
using System.Reflection;

namespace Shared.Infrastructure.Database;

public static class MigrationStartup
{
    private static readonly string[] KnownInfrastructureAssemblies =
    [
        "Identity.Infrastructure",
        "Shop.Infrastructure",
        "PlayerProgressStore.Infrastructure"
    ];

    public static async Task ApplyAllMigrationsAsync(this WebApplication app)
    {
        var isMigrateOnly = string.Equals(Environment.GetEnvironmentVariable("MIGRATE_ONLY"), "true", StringComparison.OrdinalIgnoreCase);

        if (!app.Environment.IsLocalOrDevelopement() && !isMigrateOnly)
            return;

        EnsureInfrastructureAssembliesLoaded();

        const int maxRetries = 3;
        const int delayMs = 2000;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await RunMigrationsAsync(app);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("EFMigrations");
                logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed. Retrying in {Delay}ms.", attempt, maxRetries, delayMs * attempt);
                await Task.Delay(delayMs * attempt);
            }
        }
    }

    private static void EnsureInfrastructureAssembliesLoaded()
    {
        var baseDir = AppContext.BaseDirectory;
        foreach (var name in KnownInfrastructureAssemblies)
        {
            try
            {
                var path = Path.Combine(baseDir, name + ".dll");
                if (File.Exists(path))
                    Assembly.LoadFrom(path);
            }
            catch { }
        }
    }

    private static async Task RunMigrationsAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("EFMigrations");

        var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .SelectMany(a =>
            {
                try { return a.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types?.Where(t => t is not null) ?? []; }
            })
            .Where(t => t is not null
                && typeof(DbContext).IsAssignableFrom(t)
                && !t.IsAbstract
                && !t.ContainsGenericParameters)
            .Distinct()
            .ToArray();

        foreach (var ctxType in dbContextTypes)
        {
            if (ctxType is null) continue;
            var ctxObj = sp.GetService(ctxType) as DbContext;
            if (ctxObj is null)
            {
                var factoryType = typeof(IDbContextFactory<>).MakeGenericType(ctxType);
                var factory = sp.GetService(factoryType);
                if (factory is not null)
                {
                    dynamic dynFactory = factory!;
                    ctxObj = (DbContext)dynFactory.CreateDbContext();
                }
            }

            if (ctxObj is null)
            {
                logger.LogDebug("Skipping DbContext {ContextType}: not registered in DI.", ctxType!.Name);
                continue;
            }

            try
            {
                logger.LogInformation("Applying migrations for DbContext {ContextType}…", ctxType!.Name);
                await ctxObj.Database.MigrateAsync();
                logger.LogInformation("Migrations applied for DbContext {ContextType}.", ctxType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed applying migrations for DbContext {ContextType}.", ctxType!.Name);
                throw;
            }
            finally
            {
                await ctxObj.DisposeAsync();
            }
        }
    }
}
