using AppHost;
using AppHost.Extensions;
using Identity.Presentation;
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

await app.UseAppHost();
await app.RunAsync();
