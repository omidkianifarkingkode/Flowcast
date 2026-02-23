using AppHost;
using AppHost.Extensions;
using Identity.Presentation;
using Shared.Infrastructure.Database;
using PlayerProgressStore.Presentation;
using Shop.Presentation;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

builder
    .ConfigureAppHost()
    .ConfigureBuildingBlocks()
    .AddIdentity()
    .AddPlayerProgress()
    .AddShop();

var app = builder.Build();

app.LogEnvironmentStartup();

if (string.Equals(Environment.GetEnvironmentVariable("MIGRATE_ONLY"), "true", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await app.ApplyAllMigrationsAsync();
        Environment.Exit(0);
    }
    catch
    {
        Environment.Exit(1);
    }
    return;
}

await app.UseAppHost();
await app.RunAsync();
