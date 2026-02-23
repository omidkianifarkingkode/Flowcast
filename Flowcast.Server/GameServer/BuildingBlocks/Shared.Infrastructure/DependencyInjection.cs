using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Application.Authentication;
using Shared.Application.Services;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Authorization;
using Shared.Infrastructure.Database;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure;


public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        return builder
            .AddServices()
            .AddHealthChecks()
            .AddAuthorizationInternal();
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        builder.Services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        builder.Services.AddMemoryCache();

        //builder.Services.AddSingleton<ILivenessProbe, RegistryLivenessProbe>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddScoped<AuditInterceptor>();

        builder.Host.UseSerilog((ctx, cfg) =>
        {
            cfg.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}");

            var seqUrl = ctx.Configuration["Serilog__SeqUrl"]
                ?? ctx.Configuration["Serilog:SeqUrl"]
                ?? ctx.Configuration["Serilog__WriteTo__1__Args__ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                try
                {
                    cfg.WriteTo.Seq(seqUrl.Trim());
                }
                catch { }
            }

        });

        return builder;
    }


    public static WebApplicationBuilder AddDbContext<T>(this WebApplicationBuilder builder, Func<IServiceProvider, DbContextSetupOptions> optionsFunc) where T : DbContext
    {
        builder.Services.AddDbContext<T>((serviceProvider, options) =>
            {
                var dbOptions = optionsFunc(serviceProvider);

                if (dbOptions.UseInMemoryDb)
                {
                    options.UseInMemoryDatabase(dbOptions.ModuleName);
                }
                else
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var provider = config["Database:Provider"] ?? "SqlServer";
                    var migrationsAssembly = typeof(T).Assembly.FullName;
                    if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
                        options.UseNpgsql(dbOptions.ConnectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
                    else
                        options.UseSqlServer(dbOptions.ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                }

                var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
                options.AddInterceptors(auditInterceptor);
            });

        return builder;
    }

    private static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder)
    {
        //builder.Services
        //    .AddHealthChecks();
        //.AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

        return builder;
    }

    private static WebApplicationBuilder AddAuthorizationInternal(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<PermissionProvider>();

        builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return builder;
    }
}
