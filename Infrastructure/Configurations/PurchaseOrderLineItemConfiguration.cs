using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PurchaseOrderLineItemConfiguration : IEntityTypeConfiguration<PurchaseOrderLineItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLineItem> builder)
    {
       builder.HasKey(li => new { li.PurchaseOrderId, li.ProductId });

       builder.Property(li => li.PurchaseOrderId)
              .IsRequired();

       builder.Property(li => li.ProductId)
              .IsRequired();

       builder.Property(li => li.Quantity)
              .IsRequired();

       builder.Property(li => li.ReceivedQuantity)
              .IsRequired();

       builder.Property(li => li.UnitCost)
              .IsRequired()
              .HasColumnType("decimal(18,2)");

       builder.ToTable(t => {
           t.HasCheckConstraint("CK_PurchaseOrderLineItem_Quantity", "[Quantity] > 0");
           t.HasCheckConstraint("CK_PurchaseOrderLineItem_UnitCost", "[UnitCost] >= 0");
       });

       builder.HasOne(li => li.Product)
              .WithMany(p => p.PurchaseOrderLineItem)
              .HasForeignKey(li => li.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

       builder.HasOne(li => li.PurchaseOrder)
              .WithMany(po => po.LineItems)
              .HasForeignKey(li => li.PurchaseOrderId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
