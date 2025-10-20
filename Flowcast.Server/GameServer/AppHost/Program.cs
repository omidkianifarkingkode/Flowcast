using AppHost.Extensions;
using Identity.Presentation;
using PlayerProgressStore.Presentation;
using Presentation;

var builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureAppHost()
    .ConfigureBuildingBlocks()
    .AddIdentity()
    .AddPlayerProgress();

var app = builder.Build();

await app.UseAppHost();

await app.RunAsync();
