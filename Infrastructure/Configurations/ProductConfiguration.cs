using Domain.Entities.Core;
using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(tl => tl.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.ToTable(tl =>
                tl.HasCheckConstraint("CK_Product_Price", "[Price] >= 0")
            );

            builder.HasOne(p => p.Company)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.InventoryPolicy)
                   .WithOne(i => i.Product)
                   .HasForeignKey<ProductInventoryPolicy>(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.StockLevel)
                   .WithOne(s => s.Product)
                   .HasForeignKey<StockLevel>(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.SalesAggregates)
                   .WithOne(s => s.Product)
                   .HasForeignKey(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Forecasts)
                   .WithOne(f => f.Product)
                   .HasForeignKey(f => f.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ConfirmedOrderAggregates)
                   .WithOne(s => s.Product)
                   .HasForeignKey(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.TransactionLineItems)
                   .WithOne(tl => tl.Product)
                   .HasForeignKey(tl => tl.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.PurchaseOrderLineItem)
                   .WithOne(l => l.Product)
                   .HasForeignKey(l => l.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.ConfirmedOrderLineItem)
                   .WithOne(l => l.Product)
                   .HasForeignKey(l => l.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => new { p.CompanyId, p.SKU }).IsUnique();
            builder.HasIndex(p => new { p.CompanyId, p.Name }).IsUnique();
        }
    }
}
