using Domain.Entities.Core;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Repositories;
using Resend;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Imports;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Scheduler;
using Infrastructure.Services.Scheduler.Imports.Handlers;
using Infrastructure.Services.Scheduler.Calculations;
using Infrastructure.Services.Users;
using Domain.Interfaces.Services.Users;
using Infrastructure.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration config
    )
    {
        // --- Database ---
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"), 
                opt => opt.CommandTimeout(900))
        );

        // --- Identity ---
        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddSignInManager<SignInManager<User>>()
        .AddDefaultTokenProviders();

        // --- JWT configuration ---
        var jwtSettings = config.GetSection("Jwt");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
                    ),
                    RoleClaimType = "role" 
                };
            });

        // --- Authorization Policies ---
        services.AddAuthorizationBuilder()
            .AddPolicy("SuperAdminOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "SuperAdmin"))
            .AddPolicy("AdminOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"))
            .AddPolicy("AdminOrSuperAdmin", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", new[] { "Admin", "SuperAdmin" }));

        // --- Repositories ---
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISalesAggregateRepository, SalesAggregateRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<IConfirmedOrderAggregateRepository, ConfirmedOrderAggregateRepository>();
        services.AddScoped<IPurchaseForecastRepository, PurchaseForecastRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobTaskRepository, JobTaskRepository>();
        services.AddScoped<IScheduledJobRepository, ScheduledJobRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

        // --- Internal Services ---
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IStockLevelCalculation, StockLevelCalculation>();
        services.AddScoped<ISalesAggregateCalculation, SalesAggregateCalculation>();
        services.AddScoped<IPurchaseForecastCalculation, PurchaseForecastCalculation>();
        services.AddScoped<IForecastCalculation, ForecastCalculation>();
        services.AddScoped<IConfirmedOrderAggregateCalculation, ConfirmedOrderAggregateCalculation>();

        services.AddScoped<IImportHandler, ProductImportHandler>();
        services.AddScoped<IImportHandler, TransactionImportHandler>();
        services.AddScoped<IImportHandler, ConfirmedOrderImportHandler>();
        services.AddScoped<IImportHandler, PurchaseOrderImportHandler>();

        services.AddScoped<IImportCoordinator, ImportCoordinator>();

        services.AddScoped<IJobDispatcher, JobDispatcher>();
        services.AddScoped<IJobRunnerService, JobRunnerService>();
        services.AddScoped<IJobTaskRunnerService, JobTaskRunnerService>();

        // --- External Services ---
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o => 
        {
            o.ApiToken = config["Resend:ApiKey"]!;
        });
        services.AddScoped<IResend, ResendClient>();

        return services;
    }
}
