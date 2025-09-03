using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Realtime.TestHost.Authentication;
using Realtime.TestHost.Authorization;
using Realtime.TestHost.Messaging;
using Realtime.Transport;
using System.Text;

namespace Realtime.TestHost;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.Services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        builder.AddRealtimeServices()
            .DiscoverMessagesFrom(typeof(DependencyInjection).Assembly)
            .UseCommandRouting(routes => routes
                .Map(typeof(ICommand), typeof(ICommandHandler<>))
                .Map(typeof(ICommand<>), typeof(ICommandHandler<,>))
            );

        builder.Services.AddOpenApi();

        builder.AddAuthenticationInternal();
        builder.AddAuthorizationInternal();

        return builder;
    }

    private static WebApplicationBuilder AddAuthenticationInternal(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                var secret = builder.Configuration["Jwt:Secret"];
                var iss = builder.Configuration["Jwt:Issuer"];
                var aud = builder.Configuration["Jwt:Audience"];

                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });


        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
        builder.Services.AddSingleton<ITokenProvider, TokenProvider>();

        return builder;
    }

    private static WebApplicationBuilder AddAuthorizationInternal(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<PermissionProvider>();

        builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return builder;
    }
}