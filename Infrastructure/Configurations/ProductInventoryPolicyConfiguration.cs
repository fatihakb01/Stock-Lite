using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ProductInventoryPolicyConfiguration
    : IEntityTypeConfiguration<ProductInventoryPolicy>
{
    public void Configure(EntityTypeBuilder<ProductInventoryPolicy> builder)
    {
        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.LeadTimeDays)
            .IsRequired();

        builder.Property(p => p.MinimumOrderQuantity)
            .IsRequired();

        builder.Property(p => p.OrderIncrementQuantity)
            .IsRequired();

        builder.Property(p => p.SafetyStock)
            .HasDefaultValue(0);

        builder.Property(p => p.StockAdjustment)
            .HasDefaultValue(0);

        builder.ToTable(p => {
            p.HasCheckConstraint("CK_ProductInventoryPolicy_LeadTimeDays", "[LeadTimeDays] BETWEEN 1 AND 365");
            p.HasCheckConstraint("CK_ProductInventoryPolicy_MinimumOrderQuantity", "[MinimumOrderQuantity] > 0");
            p.HasCheckConstraint("CK_ProductInventoryPolicy_OrderIncrementQuantity", "[OrderIncrementQuantity] > 0");
        });
    }
}
