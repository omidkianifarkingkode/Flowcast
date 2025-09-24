using Identity.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityService(builder.Configuration, setup =>
{
    setup.ConfigureJwtBearer = true;
    setup.ConfigureApiVersioning = true;
    setup.ContributeSwagger = true;
    setup.ValidateHostPrereqs = true;
});

var app = builder.Build();

await app.UseIdentity(setup =>
{
    setup.MigrateOnStartup = true;
    setup.SeedOnStartup = true;
    setup.ContributeSwagger = true;
    setup.UseAuthorization = true;
});

//app.MapGet("/_debug/routes", (IEnumerable<EndpointDataSource> sources) =>
//{
//    var lines = sources
//        .SelectMany(s => s.Endpoints)
//        .Select(e => e.DisplayName);
//    return Results.Text(string.Join("\n", lines));
//});

app.Run();
