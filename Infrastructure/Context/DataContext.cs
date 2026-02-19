using Domain.Entities.Core;
using Domain.Entities.Derived;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class DataContext(DbContextOptions<DataContext> options) 
    : IdentityDbContext<User>(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductInventoryPolicy> ProductInventoryPolicies { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionLineItem> TransactionLineItems { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrder { get; set; }
    public DbSet<PurchaseOrderLineItem> PurchaseOrderLineItem { get; set; }
    public DbSet<ConfirmedOrder> ConfirmedOrder { get; set; }
    public DbSet<ConfirmedOrderLineItem> ConfirmedOrderLineItem { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<PurchaseForecast> PurchaseForecasts { get; set; }
    public DbSet<SalesAggregate> SalesAggregates { get; set; }
    public DbSet<Forecast> Forecasts { get; set; }
    public DbSet<Seasonality> Seasonality { get; set; }
    public DbSet<ConfirmedOrderAggregate> ConfirmedOrderAggregates { get; set; }
    public DbSet<JobTask> JobTasks { get; set; }
    public DbSet<ScheduledJob> ScheduledJobs { get; set; }
    public DbSet<JobExecution> JobExecutions { get; set; }
    public DbSet<ProductKpi> ProductKpis { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity table overrides
        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Making sure that Configurations are applied to database
        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}

