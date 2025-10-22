using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Persistences;
using Identity.Infrastructure.Persistences.Repositories;
using Identity.Infrastructure.Services;
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
using Shared.Application.Services;
using Shared.Infrastructure.Database;
using SharedKernel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder
            .SetupOptions()
            .AddPersistences()
            .AddServices()
            .AddAuth();

        return builder;
    }

    private static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<IdentityOptions>()
                .BindConfiguration("Identity")
                .ValidateDataAnnotations()
                .Validate(o => o is not null, "Identity options missing")
                .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<IdentityOptions>, IdentityOptionsValidator>();

        return builder;
    }

    private static WebApplicationBuilder AddPersistences(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        {
            var identitySection = builder.Configuration.GetSection(IdentityOptions.SectionName);
            var opts = identitySection.Get<IdentityOptions>()!;
            var useInMemory = identitySection.GetValue<bool>("UseInMemoryDatabase");

            var connectionString = builder.Configuration.GetModuleConnectionString(IdentityOptions.SectionName);

            if (useInMemory)
            {
                opt.UseInMemoryDatabase("Identity");
            }
            else
            {
                opt.UseSqlServer(connectionString);
            }
        });

        builder.Services.AddKeyedScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>("identity");
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
        builder.Services.AddScoped<IIdentityLoginAuditRepository, IdentityLoginAuditRepository>();
        builder.Services.AddScoped<IKeyRepository, KeyRepository>();

        return builder;
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddKeyedSingleton<IProviderTokenVerifier, GoogleTokenVerifier>("google");
        builder.Services.AddScoped<IKeyStore, DbKeyStore>();

        return builder;
    }

    private static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { });

        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IServiceProvider, IOptions<IdentityOptions>>(ConfigureJwtBearer);

        //builder.Services.AddAuthorization(options =>
        //{
        //    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        //        .RequireAuthenticatedUser()
        //        .RequireClaim("token_use", "access")
        //        .Build();
        //});

        builder.Services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("token_use", "access")
                .Build());

        return builder;
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
