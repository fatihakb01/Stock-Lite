using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PurchaseOrderLineItemConfiguration : IEntityTypeConfiguration<PurchaseOrderLineItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLineItem> builder)
    {
        builder.HasKey(li => new { li.PurchaseOrderId, li.ProductId });

        builder.Property(li => li.Quantity)
               .IsRequired();

        builder.Property(li => li.UnitCost)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.HasOne(li => li.Product)
            .WithMany(p => p.PurchaseOrderLineItem)
            .HasForeignKey(li => li.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.PurchaseOrder)
            .WithMany(p => p.LineItems)
            .HasForeignKey(li => li.PurchaseOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable(t => {
            t.HasCheckConstraint("CK_PurchaseOrderLineItem_Quantity", "[Quantity] > 0");
            t.HasCheckConstraint("CK_PurchaseOrderLineItem_UnitCost", "[UnitCost] >= 0");
        });
    }
}
