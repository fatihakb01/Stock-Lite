using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Export;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Scheduler.Imports;
using Domain.Interfaces.Services.Users;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure.Services.Export;
using Infrastructure.Services.Redis;
using Infrastructure.Services.Redis.Helper;
using Infrastructure.Services.Scheduler;
using Infrastructure.Services.Scheduler.Calculations;
using Infrastructure.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports.Handlers;
using Infrastructure.Services.Scheduler.Imports.Helper;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;

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
                opt => opt.CommandTimeout(900).EnableRetryOnFailure())
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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });

        // --- Authorization Policies ---
        services.AddAuthorizationBuilder()
            .AddPolicy("SuperAdminOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "SuperAdmin"))
            .AddPolicy("AdminOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"))
            .AddPolicy("AdminOrSuperAdmin", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", ["Admin", "SuperAdmin"]))
            .AddPolicy("KeyUserOrAdminOrSuperAdmin", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", ["KeyUser", "Admin", "SuperAdmin"]));

        // --- Repositories ---
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISalesAggregateRepository, SalesAggregateRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<ISeasonalityRepository, SeasonalityRepository>();
        services.AddScoped<IConfirmedOrderAggregateRepository, ConfirmedOrderAggregateRepository>();
        services.AddScoped<IPurchaseForecastRepository, PurchaseForecastRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductKpiRepository, ProductKpiRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobTaskRepository, JobTaskRepository>();
        services.AddScoped<IScheduledJobRepository, ScheduledJobRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddScoped<IJobTaskExecutionRepository, JobTaskExecutionRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // --- Internal Services ---
        services.AddSingleton<ITokenService, TokenService>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IStockLevelCalculation, StockLevelCalculation>();
        services.AddScoped<ISalesAggregateCalculation, SalesAggregateCalculation>();
        services.AddScoped<IPurchaseForecastCalculation, PurchaseForecastCalculation>();
        services.AddScoped<IForecastCalculation, ForecastCalculation>();
        services.AddScoped<ISeasonalityCalculation, SeasonalityCalculation>();
        services.AddScoped<IConfirmedOrderAggregateCalculation, ConfirmedOrderAggregateCalculation>();
        services.AddScoped<IProductKpiCalculation, ProductKpiCalculation>();

        services.AddScoped<IImportHandler, ProductImportHandler>();
        services.AddScoped<IImportHandler, TransactionImportHandler>();
        services.AddScoped<IImportHandler, ConfirmedOrderImportHandler>();
        services.AddScoped<IImportHandler, PurchaseOrderImportHandler>();
        services.AddScoped<IImportHandler, UserDefinedFieldImportHandler>();

        services.AddScoped<IImportHelper, ImportHelper>();
        services.AddScoped<IImportCoordinator, ImportCoordinator>();

        services.AddScoped<IJobDispatcher, JobDispatcher>();
        services.AddScoped<IJobRunnerService, JobRunnerService>();
        services.AddScoped<IJobTaskDispatcher, JobTaskDispatcher>();
        services.AddScoped<IJobTaskRunnerService, JobTaskRunnerService>();
        services.AddHostedService<SchedulerWorker>();

        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IFileExportHelper, FileExportHelper>();

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ISchedulerCacheService, SchedulerCacheService>();
        services.AddSingleton<ICacheHelper, CacheHelper>();
        services.AddSingleton<IBaseCacheService<Product>, ProductCacheService>();
        services.AddSingleton<IBaseCacheService<Transaction>, TransactionCacheService>();
        services.AddSingleton<IBaseCacheService<PurchaseOrder>, PurchaseOrderCacheService>();
        services.AddSingleton<IBaseCacheService<ConfirmedOrder>, ConfirmedOrderCacheService>();
        services.AddSingleton<IBaseCacheService<UserDefinedField>, UserDefinedFieldCacheService>();
        services.AddSingleton<IBaseCacheService<User>, UserCacheService>();
        services.AddSingleton<IBaseCacheService<ExportProfile>, ExportProfileCacheService>();
        services.AddSingleton<IBaseCacheService<ProductKpi>, ProductKpiCacheService>();
        services.AddSingleton<IBaseCacheService<Forecast>, ForecastCacheService>();

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
