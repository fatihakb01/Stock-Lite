using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(po => po.Status)
               .IsRequired();

        builder.Property(po => po.OrderDate)
               .IsRequired()
               .HasColumnType("date");

        builder.Property(po => po.ReceivedDate)
               .HasColumnType("date");

        builder.Property(po => po.ExpectedDeliveryDate)
               .HasColumnType("date");

        builder.HasOne(po => po.Company)
            .WithMany(c => c.PurchaseOrders)
            .HasForeignKey(po => po.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.LineItems)
               .WithOne(li => li.PurchaseOrder)
               .HasForeignKey(li => li.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(po => new { po.CompanyId, po.OrderNumber }).IsUnique();
    }
}

