using Matchmaking.Contracts;
using Matchmaking.Infrastructure;
using MatchMaking.Application.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddMatchmakingInfrastructure(this WebApplicationBuilder builder)
    {
        return builder
            .AddServices()
            .AddDatabase()
            .AddHealthChecks();
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ITicketRepository, InMemoryTicketRepository>();
        builder.Services.AddScoped<IMatchRepository, InMemoryMatchRepository>();
        builder.Services.AddScoped<IMatchmakingNotifier, MatchmakingNotifier>();

        return builder;
    }

    private static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("Database");

        //builder.Services.AddDbContext<ApplicationDbContext>(
        //    options => options
        //        //.UseNpgsql(connectionString, npgsqlOptions =>
        //        //    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
        //        .UseSnakeCaseNamingConvention());

        //builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return builder;
    }

    private static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder)
    {
        return builder;
    }
}
