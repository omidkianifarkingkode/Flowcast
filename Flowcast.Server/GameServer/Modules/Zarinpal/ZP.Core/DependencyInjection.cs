using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application;
using Shared.Application.Services;
using Shared.Infrastructure.Database;
using ZP.Core.Gateway;
using ZP.Core.Options;
using ZP.Core.Persisntence;
using ZP.Core.Repositories;
using ZP.Core.Services;

namespace ZP.Core;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddCore(this WebApplicationBuilder builder)
    {
        builder
            .SetupOptions()
            .AddPersistances()
            .AddGateway()
            .AddCQRS(Assembly.GetExecutingAssembly());
        if (!builder.Services.Any(s => s.ServiceType == typeof(IDateTimeProvider)))
            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddScoped<IPaymentRequestRepository, PaymentRequestRepository>();
        return builder;
    }


    private static WebApplicationBuilder AddGateway(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IZarinpalGateway, ZarinpalGateway>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ZarinpalOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl!.TrimEnd('/') + "/");
        });
        return builder;
    }
    private static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<ZarinpalOptions>()
            .BindConfiguration(ZarinpalOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(x => x is not null, "Zarinpal options not found")
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<ZarinpalOptions>, ZarinpalOptionsValidator>();
        return builder;
    }
    private static WebApplicationBuilder AddPersistances(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        {
            var zarinpalOptions = builder.Configuration
                .GetSection(ZarinpalOptions.SectionName)
                .Get<ZarinpalOptions>()
                ?? throw new InvalidOperationException("ZarinPal options not found");

            if(zarinpalOptions.UseInMemoryDatabase)
            {
                opt.UseInMemoryDatabase("zarinpal");
            }
            else
            {
                if(string.IsNullOrWhiteSpace(zarinpalOptions.ConnectionStrings))
                    throw new InvalidOperationException("Zarinpal connection string not configured");

                opt.UseSqlServer(zarinpalOptions.ConnectionStrings, sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
            }
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();
        
        return builder;
    }
}
