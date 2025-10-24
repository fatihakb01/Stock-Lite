using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Domain.Interfaces;
using MediatR;
using API.Middleware;
using Application.Behaviors;

namespace API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration config
    )
    {
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
        );

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped(typeof(IUniquenessChecker<>), typeof(UniquenessChecker<>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<ExceptionMiddleware>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
