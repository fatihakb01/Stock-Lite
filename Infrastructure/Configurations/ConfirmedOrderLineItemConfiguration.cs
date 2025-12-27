using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ConfirmedOrderLineItemConfiguration : IEntityTypeConfiguration<ConfirmedOrderLineItem>
{
    public void Configure(EntityTypeBuilder<ConfirmedOrderLineItem> builder)
    {
        builder.HasKey(li => new { li.ConfirmedOrderId, li.ProductId });

        builder.Property(li => li.Quantity)
            .IsRequired();

        builder.Property(li => li.SoldQuantity)
            .IsRequired();

       builder.Property(li => li.UnitCost)
              .IsRequired()
              .HasColumnType("decimal(18,2)");

        builder.ToTable(t => {
            t.HasCheckConstraint("CK_ConfirmedOrderLineItem_Quantity", "[Quantity] > 0");
            t.HasCheckConstraint("CK_ConfirmedOrderLineItem_SoldQuantity", "[SoldQuantity] >= 0");
            t.HasCheckConstraint("CK_ConfirmedOrderLineItem_UnitCost", "[UnitCost] >= 0");
        });

       builder.HasOne(li => li.Product)
              .WithMany(c => c.ConfirmedOrderLineItem)
              .HasForeignKey(li => li.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

       builder.HasOne(li => li.ConfirmedOrder)
              .WithMany(po => po.LineItems)
              .HasForeignKey(li => li.ConfirmedOrderId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
