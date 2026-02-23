using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Shared.Application.Services;
using Shared.Infrastructure;
using Shared.Infrastructure.Database;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Infrastructure.Options;
using Shop.Infrastructure.Persistences;
using Shop.Infrastructure.Persistences.Repositories;
using Shop.Infrastructure.Services;

namespace Shop.Infrastructure;

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
        builder.Services.AddOptions<ShopOptions>()
            .BindConfiguration(ShopOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(x => x is not null, "Shop options not found")
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<ShopOptions>, ShopOptionsValidator>();

        return builder;
    }
    private static WebApplicationBuilder AddPersistances(this WebApplicationBuilder builder)
    {
        builder.AddDbContext<ApplicationDbContext>((serviceProvider) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ShopOptions>>().Value;

            var connectionString = options.ConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

            return new DbContextSetupOptions(connectionString, "shop", options.UseInMemoryDatabase);
        });

        builder.Services.AddKeyedScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>("shop");
        builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        return builder;
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IPurchaseValidationService, PurchaseValidationService>(client =>
        {
            client.BaseAddress = new Uri("https://eu-kkvr.kingcodestudio.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)));

        builder.Services.AddHostedService<PurchaseValidationWorker>();

        return builder;
    }
}