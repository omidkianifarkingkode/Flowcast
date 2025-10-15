using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlayerProgressStore.Application;
using PlayerProgressStore.Infrastructure.Persistence;
using PlayerProgressStore.Infrastructure.Services;
using Shared.Application.Services;
using Shared.Infrastructure.Database;

namespace PlayerProgressStore.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPlayerProgressStore(
            this WebApplicationBuilder builder)
        {
            SetupOptions(builder.Services);

            // DbContext (Scoped)
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            {
                var section = builder.Configuration.GetSection(PlayerProgressOptions.SectionName);
                var opts = section.Get<PlayerProgressOptions>()!;
                var useInMemory = section.GetValue<bool>(nameof(PlayerProgressOptions.UseInMemoryDatabase));

                if (useInMemory)
                {
                    opt.UseInMemoryDatabase("PlayerProgress");
                }
                else
                {
                    opt.UseSqlServer(opts.ConnectionString!, sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
                }
            });

            builder.Services.AddScoped<IPlayerNamespaceRepository, PlayerNamespaceRepository>();
            builder.Services.AddKeyedScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>("playerprogress");

            // Core services (Singleton)
            builder.Services.AddSingleton<ICanonicalJsonService, CanonicalJsonService>();
            builder.Services.AddSingleton<IContentHashService, ContentHashService>();
            builder.Services.AddSingleton<IVersionTokenService, VersionTokenService>();
            builder.Services.AddSingleton<INamespaceValidationPolicy, NamespaceValidationPolicy>();
            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            // Merge resolvers & registry (Singleton)
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

        private static void SetupOptions(IServiceCollection services)
        {
            services.AddOptions<PlayerProgressOptions>()
                .BindConfiguration(PlayerProgressOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(o => o is not null, "PlayerProgress options missing")
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<PlayerProgressOptions>, PlayerProgressOptionsValidator>();
        }
    }
}
