using Asp.Versioning.ApiExplorer;
using Identity.API.Businesses.Commands;
using Identity.API.Infrastructures;
using Identity.API.Options;
using Identity.API.Persistence;
using Identity.API.Persistence.Repositories;
using Identity.API.Services;
using Identity.API.Services.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Shared.API.Swagger;
using Shared.API.Versioning;
using Shared.Application.Services;
using SharedKernel;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Identity.API.Extensions;

public static class AddModuleExtension
{
    public sealed class IdentityInfraOptions
    {
        public bool UseMemoryCache { get; set; } = true;
        public bool ConfigureJwtBearer { get; set; } = true;
        public bool ContributeSwagger { get; set; } = false;
        public bool ConfigureApiVersioning { get; set; } = false;
        public bool ValidateHostPrereqs { get; set; } = false;
    }

    public static WebApplicationBuilder AddIdentityService(this WebApplicationBuilder builder, Action<IdentityInfraOptions>? configure = null)
    {
        var infra = new IdentityInfraOptions();
        configure?.Invoke(infra);

        SetupOptions(builder.Services);
        AddBusinessCommands(builder.Services);
        AddServices(builder.Services);
        AddPersistences(builder.Services, builder.Configuration);

        AddInfrastructure(builder.Services, infra);

        return builder;
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

    private static void AddInfrastructure(IServiceCollection services, IdentityInfraOptions infra)
    {
        if (infra.UseMemoryCache)
            services.AddMemoryCache();

        if (infra.ConfigureApiVersioning)
            services.InstallVersioning();

        if (infra.ConfigureJwtBearer)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { });

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IServiceProvider, IOptions<IdentityOptions>>(ConfigureJwtBearer);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireClaim("token_use", "access")
                    .Build();
            });
        }

        if (infra.ContributeSwagger)
        {
            services.AddSwaggerGen();

            // host still calls AddSwaggerGen(); this only adds Identity-specific filters/options
            services.Configure<SwaggerGenOptions>(o =>
            {
                // e.g., add operation/schema filters for Identity endpoints if you have any
                // _.OperationFilter<IdentityAuthHeaderFilter>();
                o.CustomSchemaIds(t => t.FullName!.Replace('+', '-'));
            });
        }

        if (infra.ValidateHostPrereqs)
        {
            // Validate by inspecting registrations (no second container)
            EnsureRegistration<IMemoryCache>(services, required: !infra.UseMemoryCache,
                "IMemoryCache is required. Host must call AddMemoryCache() or set UseMemoryCache=true.");

            EnsureRegistration<IOptionsMonitor<JwtBearerOptions>>(services, required: !infra.ConfigureJwtBearer,
                "JwtBearer is required. Host must AddAuthentication().AddJwtBearer() or set ConfigureJwtBearer=true.");

            EnsureRegistration<IApiVersionDescriptionProvider>(services, required: !infra.ConfigureApiVersioning,
                "API Versioning is required. Host must AddApiVersioning().AddApiExplorer() or set ConfigureApiVersioning=true.");
        }
    }

    private static void EnsureRegistration<T>(IServiceCollection services, bool required, string message)
    {
        if (!required) return;
        var exists = services.Any(sd => sd.ServiceType == typeof(T));
        if (!exists) throw new InvalidOperationException(message);
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<GoogleTokenVerifier>();
        services.AddKeyedSingleton<IProviderTokenVerifier>("google", (sp, _) => sp.GetRequiredService<GoogleTokenVerifier>());
        services.AddScoped<IKeyStore, DbKeyStore>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
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

        var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Auth");

        o.RequireHttpsMetadata = true;

        // --- Token validation ---
        o.TokenValidationParameters = new TokenValidationParameters
        {
            // Issuer / Audience
            ValidateIssuer = true,
            ValidIssuer = t.Issuer,
            ValidateAudience = true,
            ValidAudience = t.Audience,

            // Signature / Lifetime
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(Math.Max(0, t.ClockSkewSeconds)),

            // Identity mapping
            NameClaimType = JwtRegisteredClaimNames.Sub,

            // Keys will be supplied dynamically:
            IssuerSigningKeyResolver = (_, _, _, _) => ResolveValidationKeys(sp)
        };

        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                log.LogError(ctx.Exception, "JWT auth failed");
                return Task.CompletedTask;
            },

            // 401 - not authenticated / invalid or expired token
            OnChallenge = async ctx =>
            {
                const string contentTypeProblemJson = "application/problem+json";
                const string codeUnauthorized = "Auth.Unauthorized";
                const string defaultMessage = "Authentication failed.";

                // Suppress default (no-body) behavior
                ctx.HandleResponse();

                if (ctx.Response.HasStarted) 
                    return;

                // Preserve WWW-Authenticate for clients
                var description = string.IsNullOrWhiteSpace(ctx.ErrorDescription) ? defaultMessage : ctx.ErrorDescription;

                // Preserve header for clients (quoted per RFC)
                ctx.Response.Headers[HeaderNames.WWWAuthenticate] = BuildBearerAuthenticateHeader(ctx.Error, description);

                // Build Error -> Result once; only description varies
                var result = Result.Failure(Error.Unauthorized(codeUnauthorized, description));

                // Write problem+json via your CustomResults
                ctx.Response.ContentType = contentTypeProblemJson;
                await CustomResults.Problem(result, ctx.HttpContext).ExecuteAsync(ctx.HttpContext);
            },

            // 403 - authenticated but not allowed
            OnForbidden = async ctx =>
            {
                const string contentTypeProblemJson = "application/problem+json";
                const string authForbiddenCode = "Auth.Forbidden";
                const string authForbiddenMessage = "You do not have permission to access this resource.";

                if (ctx.Response.HasStarted) return;

                var result = Result.Failure(Error.Forbidden(authForbiddenCode, authForbiddenMessage));

                ctx.Response.ContentType = contentTypeProblemJson;
                await CustomResults.Problem(result, ctx.HttpContext).ExecuteAsync(ctx.HttpContext);
            },

            OnTokenValidated = ctx =>
            {
                log.LogInformation("Token validated. sub={Sub}, kid={Kid}",
                ctx.Principal!.FindFirst("sub")?.Value,
                (ctx.SecurityToken as JwtSecurityToken)?.Header.Kid);

                var use = ctx.Principal?.FindFirst("token_use")?.Value;
                if (!string.Equals(use, "access", StringComparison.Ordinal))
                {
                    ctx.Fail("Wrong token type (refresh token presented where access token required).");
                }

                return Task.CompletedTask;
            }
        };

        static string BuildBearerAuthenticateHeader(string? error, string description)
        => string.IsNullOrWhiteSpace(error)
           ? $"Bearer error_description={Escape(description)}"
           : $"Bearer error={Escape(error)}, error_description={Escape(description)}";

        static string Escape(string s) => s.Replace("\"", "\\\"");
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


    private static SecurityKey[] ResolveValidationKeys(IServiceProvider sp)
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
    }

}
