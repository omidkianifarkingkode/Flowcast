using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Extensions;
using System.Reflection;

namespace Shared.Infrastructure.Database;

public static class MigrationStartup
{
    public static async Task ApplyAllMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsLocalOrDevelopement())
            return;

        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("EFMigrations");

        // Find all non-abstract DbContext types in loaded assemblies
        var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try { return a.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t is not null)!; }
            })
            .Where(t => t is not null
                        && typeof(DbContext).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.ContainsGenericParameters)
            .Distinct()
            .ToArray();

        foreach (var ctxType in dbContextTypes)
        {
            // Try resolve the context directly
            var ctxObj = sp.GetService(ctxType) as DbContext;
            if (ctxObj is null)
            {
                // Alternatively, try an IDbContextFactory<TContext> if you register factories
                var factoryType = typeof(IDbContextFactory<>).MakeGenericType(ctxType);
                var factory = sp.GetService(factoryType);
                if (factory is not null)
                {
                    // dynamic usage to avoid reflection invoke boilerplate
                    dynamic dynFactory = factory!;
                    ctxObj = (DbContext)dynFactory.CreateDbContext();
                }
            }

            if (ctxObj is null)
            {
                logger.LogDebug("Skipping DbContext {ContextType}: not registered in DI.", ctxType.Name);
                continue;
            }

            try
            {
                logger.LogInformation("Applying migrations for DbContext {ContextType}…", ctxType.Name);
                await ctxObj.Database.MigrateAsync();
                logger.LogInformation("Migrations applied for DbContext {ContextType}.", ctxType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed applying migrations for DbContext {ContextType}.", ctxType.Name);
                // Optionally rethrow if you want startup to fail:
                // throw;
            }
            finally
            {
                await ctxObj.DisposeAsync();
            }
        }
    }
}
