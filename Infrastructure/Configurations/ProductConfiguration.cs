using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.SKU).IsRequired().HasMaxLength(100);
            builder.Property(tl => tl.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

            builder.ToTable(tl =>
                tl.HasCheckConstraint("CK_Product_Price", "[Price] >= 0")
            );

            builder.HasOne(p => p.StockLevel)
                   .WithOne(s => s.Product)
                   .HasForeignKey<StockLevel>(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
