using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Services;
using Shared.Infrastructure.Database;
using Shop.Application.IRepositories;
using Shop.Infrastructure.Options;
using Shop.Infrastructure.Persistences;
using Shop.Infrastructure.Persistences.Repositories;

namespace Shop.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder
            .SetupOptions()
            .AddPersistances();

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
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        {
            var shopOptions = builder.Configuration
                .GetSection(ShopOptions.SectionName)
                .Get<ShopOptions>()
                ?? throw new InvalidOperationException("Shop options not found");

            if(shopOptions.UseInMemoryDatabase)
            {
                opt.UseInMemoryDatabase("shop");
            }
            else
            {
                if(string.IsNullOrWhiteSpace(shopOptions.ConnectionStrings))
                    throw new InvalidOperationException("Shop connection string not configured");

                opt.UseSqlServer(shopOptions.ConnectionStrings, sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
            }
        });

        builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();

        return builder;
    }

}
