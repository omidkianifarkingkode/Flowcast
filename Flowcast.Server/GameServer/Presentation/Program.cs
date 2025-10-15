using Presentation;
using Presentation.Extensions;
using Identity.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAppHost();

builder.AddIdentityService();
builder.AddPlayerProgressStore();

var app = builder.Build();

await app.UseAppHost();

await app.RunAsync();
