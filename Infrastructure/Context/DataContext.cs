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
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionLineItem> TransactionLineItems { get; set; }
    // public override DbSet<User> Users { get; set; }
    public DbSet<SalesAggregate> SalesAggregates { get; set; }

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

