using Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Context;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionLineItem> TransactionLineItems { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Making sure that Configurations are applied to database
        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}

