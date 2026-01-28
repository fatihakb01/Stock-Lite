using Infrastructure.Context;
using Application.Core;
using API.Extensions;
using API.Middleware;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

await app.InitializeAndSeedDatabaseAsync();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicyName");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();