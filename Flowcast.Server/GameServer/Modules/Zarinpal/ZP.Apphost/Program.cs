using Shared.Presentation.Endpoints;
using Shared.Presentation.Swagger;
using Shared.Presentation.Versioning;
using ZP.Apphost;

var builder = WebApplication.CreateBuilder(args);

builder.Services.InstallVersioning();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddZarinpal();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
if (app.Environment.IsDevelopment())
    app.UseSwagger();
app.MapEndpoints();
app.Run();
