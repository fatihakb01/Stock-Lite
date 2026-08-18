using API.Hubs;
using API.Middleware;
using Application.Behaviors;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

namespace API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration config
    )
    {
        // ---------- CONTROLLERS & JSON ----------
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });

        // ---------- OPENAPI ----------
        services.AddOpenApi();

        // ---------- CORS ----------
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicyName", policy =>
            {
                policy.WithOrigins(config["ClientAppUrl"]!)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // ---------- MEDIATOR & PIPELINES ----------
        services.AddTransient(
            typeof(IPipelineBehavior<,>), 
            typeof(ValidationBehavior<,>)
        );

        // ---------- MIDDLEWARE ----------
        services.AddTransient<ExceptionMiddleware>();

        // ---------- SIGNALR ----------
        services.AddSignalR();

        services.AddKeyedScoped<INotificationService>("company", (provider, key) =>
        {
            var hubContext = provider.GetRequiredService<IHubContext<CompanyHub>>();
            return new NotificationService(hubContext.Clients);
        });

        services.AddKeyedScoped<INotificationService>("product", (provider, key) =>
        {
            var hubContext = provider.GetRequiredService<IHubContext<ProductHub>>();
            return new NotificationService(hubContext.Clients);
        });

        services.AddScoped<ICompanyNotificationService>(provider =>
        {
            var notificationService = provider.GetRequiredKeyedService<INotificationService>("company");
            return new CompanyNotificationService(notificationService);
        });

        services.AddScoped<IProductNotificationService>(provider =>
        {
            var notificationService = provider.GetRequiredKeyedService<INotificationService>("product");
            return new ProductNotificationService(notificationService);
        });

        // ---------- REDIS CACHING ----------
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config["Redis"];
            options.InstanceName = "StockLite:";
        });

        return services;
    }
}
