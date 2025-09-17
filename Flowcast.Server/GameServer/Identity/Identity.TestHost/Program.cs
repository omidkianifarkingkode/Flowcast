using Identity.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityService(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

await app.UseIdentity();

//app.MapGet("/_debug/routes", (IEnumerable<EndpointDataSource> sources) =>
//{
//    var lines = sources
//        .SelectMany(s => s.Endpoints)
//        .Select(e => e.DisplayName);
//    return Results.Text(string.Join("\n", lines));
//});

app.Run();
