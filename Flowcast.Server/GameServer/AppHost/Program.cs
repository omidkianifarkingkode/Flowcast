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
    await app.ApplyAllMigrationsAsync();
    return;
}

await app.UseAppHost();
await app.RunAsync();
