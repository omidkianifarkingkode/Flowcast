using Identity.API.Extensions;
using PlayerProgressStore.Presentation;
using Presentation;
using Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAppHost();

builder.AddIdentityService();
builder.AddPlayerProgress();

var app = builder.Build();

await app.UseAppHost();

await app.RunAsync();
