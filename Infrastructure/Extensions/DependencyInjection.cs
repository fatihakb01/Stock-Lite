using Domain.Entities.Core;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Resend;

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
                opt => opt.CommandTimeout(300))
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

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.RoleClaimType = "role";
            options.ClaimsIdentity.UserIdClaimType = "nameid";
            options.ClaimsIdentity.UserNameClaimType = "unique_name";
            options.ClaimsIdentity.EmailClaimType = "email";
        });

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
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISalesAggregateRepository, SalesAggregateRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // --- Internal Services ---
        services.AddScoped(typeof(IUniquenessChecker<>), typeof(UniquenessChecker<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

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
