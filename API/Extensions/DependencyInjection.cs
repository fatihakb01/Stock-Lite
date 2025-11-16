using MediatR;
using API.Middleware;
using Application.Behaviors;

namespace API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration config
    )
    {
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
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<ExceptionMiddleware>();

        return services;
    }
}
