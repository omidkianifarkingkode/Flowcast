using Application;
using Infrastructure;
using Presentation;
using Presentation.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

app.SetupMiddlewares();

await app.RunAsync();
