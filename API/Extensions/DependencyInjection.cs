using MediatR;
using API.Middleware;
using Application.Behaviors;
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

        return services;
    }
}
