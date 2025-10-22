using AppHost;
using AppHost.Extensions;
using Identity.Presentation;
using PlayerProgressStore.Presentation;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

builder
    .ConfigureAppHost()
    .ConfigureBuildingBlocks()
    .AddIdentity()
    .AddPlayerProgress();

var app = builder.Build();

app.LogEnvironmentStartup();

await app.UseAppHost();
await app.RunAsync();
