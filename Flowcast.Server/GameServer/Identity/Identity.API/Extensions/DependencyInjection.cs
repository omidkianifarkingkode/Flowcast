using Asp.Versioning;
using Identity.API.Businesses.Commands;
using Identity.API.Endpoints;
using Identity.API.Infrastructures;
using Identity.API.Options;
using Identity.API.Persistence;
using Identity.API.Persistence.Repositories;
using Identity.API.Services;
using Identity.API.Services.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using System.Security.Cryptography;
using Toolkit.Swagger;
using Toolkit.Versioning;

namespace Identity.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        SetupOptions(services);
        AddInfrastructure(services);
        AddBusinessCommands(services);
        AddServices(services);
        AddPersistences(services, configuration);

        return services;
    }

    public static async Task UseIdentity(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await db.Database.MigrateAsync();

            app.UseSwagger();
        }

        MapEndpoints(app);

        await SeedDatabase(app);
    }

    // -------------------- private helpers --------------------

    private static void SetupOptions(IServiceCollection services)
    {
        services.AddOptions<IdentityOptions>()
                    .BindConfiguration("Identity")
                    .ValidateDataAnnotations()
                    .Validate(o => o is not null, "Identity options missing")
                    .ValidateOnStart();

        services.AddSingleton<IValidateOptions<IdentityOptions>, IdentityOptionsValidator>();
    }

    private static void AddInfrastructure(IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.AddMemoryCache();
        services.InstallVersioning();

        services.AddAuthorization();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IServiceProvider, IOptions<IdentityOptions>>(ConfigureJwtBearer);
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<GoogleTokenVerifier>();
        services.AddKeyedSingleton<IProviderTokenVerifier>("google", (sp, _) => sp.GetRequiredService<GoogleTokenVerifier>());
        services.AddScoped<IKeyStore, DbKeyStore>();

        services.AddSingleton<IDateTimeProvider, SharedKernel.Time.DateTimeProvider>();
    }

    private static void AddBusinessCommands(IServiceCollection services)
    {
        services.AddScoped<GoogleSignInCommandHandler>();
        services.AddScoped<DeviceSignInCommandHandler>();
        services.AddScoped<GetProfileQueryHandler>();
        services.AddScoped<LinkCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<RefreshCommandHandler>();
    }

    private static void ConfigureJwtBearer(JwtBearerOptions o, IServiceProvider sp, IOptions<IdentityOptions> opt)
    {
        var t = opt.Value.TokenOptions;

        o.RequireHttpsMetadata = true;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = t.Issuer,

            ValidateAudience = true,
            ValidAudience = t.Audience,

            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                using var scope = sp.CreateScope();
                var keyStore = scope.ServiceProvider.GetRequiredService<IKeyStore>();
                var materials = keyStore.GetValidationSetAsync(default).GetAwaiter().GetResult();

                return materials.Select(km =>
                {
                    if (km.Algorithm.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
                    {
                        var rsa = RSA.Create();
                        rsa.ImportFromPem(km.PublicKeyPem.AsSpan());
                        return (SecurityKey)new RsaSecurityKey(rsa) { KeyId = km.Kid };
                    }
                    else
                    {
                        var ec = ECDsa.Create();
                        ec.ImportFromPem(km.PublicKeyPem.AsSpan());
                        return (SecurityKey)new ECDsaSecurityKey(ec) { KeyId = km.Kid };
                    }
                }).ToArray();
            },

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(Math.Max(0, t.ClockSkewSeconds)),
        };
    }

    private static IServiceCollection AddPersistences(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(opt =>
        {
            var identitySection = configuration.GetSection("Identity");
            var opts = identitySection.Get<IdentityOptions>()!;
            var useInMemory = identitySection.GetValue<bool>("UseInMemoryDatabase");

            if (useInMemory)
            {
                opt.UseInMemoryDatabase("Identity");
            }
            else
            {
                opt.UseSqlServer(opts.ConnectionString);
            }
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IIdentityLoginAuditRepository, IdentityLoginAuditRepository>();
        services.AddScoped<IKeyRepository, KeyRepository>();


        return services;
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
