using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Infrastructure.Options;
using PlayerProgressStore.Infrastructure.Persistences;
using PlayerProgressStore.Infrastructure.Services;
using Shared.Application.Services;
using Shared.Infrastructure.Database;

namespace PlayerProgressStore.Infrastructure
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
        {
            builder
                .SetupOptions()
                .AddPersistances()
                .AddServices();

            return builder;
        }

        private static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<PlayerProgressOptions>()
                .BindConfiguration(PlayerProgressOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(o => o is not null, "PlayerProgress options missing")
                .ValidateOnStart();

            builder.Services.AddSingleton<IValidateOptions<PlayerProgressOptions>, PlayerProgressOptionsValidator>();

            return builder;
        }

        private static WebApplicationBuilder AddPersistances(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            {
                var section = builder.Configuration.GetSection(PlayerProgressOptions.SectionName);
                var opts = section.Get<PlayerProgressOptions>()!;
                var useInMemory = section.GetValue<bool>(nameof(PlayerProgressOptions.UseInMemoryDatabase));

                var connectionString = builder.Configuration.GetModuleConnectionString(PlayerProgressOptions.SectionName);

                if (useInMemory)
                {
                    opt.UseInMemoryDatabase("PlayerProgress");
                }
                else
                {
                    opt.UseSqlServer(connectionString, sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
                }
            });

            builder.Services.AddScoped<IPlayerNamespaceRepository, PlayerNamespaceRepository>();
            builder.Services.AddKeyedScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>("playerprogress");

            return builder;
        }

        private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ICanonicalJsonService, CanonicalJsonService>();
            builder.Services.AddSingleton<IContentHashService, ContentHashService>();
            builder.Services.AddSingleton<IVersionTokenService, VersionTokenService>();
            builder.Services.AddSingleton<INamespaceValidationPolicy, NamespaceValidationPolicy>();

            builder.Services.AddSingleton<IMergeResolverRegistry>(sp =>
            {
                var reg = new MergeResolverRegistry();
                // register namespace-specific resolvers here as needed:
                // reg.Register(new PlayerStatsMergeResolver());
                // reg.Register(new InventoryMergeResolver());
                // reg.Register(new SettingsMergeResolver());
                // Default resolver is internal to the registry (fallback)
                return reg;
            });

            return builder;
        }
    }
}
