using API.Extensions;
using API.Hubs;
using API.Middleware;
using Application.Core;
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

app.MapHub<CompanyHub>("/hubs/company");
app.MapHub<ProductHub>("/hubs/product");

app.Run();