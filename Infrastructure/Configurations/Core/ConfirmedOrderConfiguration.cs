using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core;

public class ConfirmedOrderConfiguration : IEntityTypeConfiguration<ConfirmedOrder>
{
    public void Configure(EntityTypeBuilder<ConfirmedOrder> builder)
    {
        builder.HasKey(co => co.Id);

        builder.Property(co => co.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(co => co.Status)
            .IsRequired();

        builder.Property(co => co.ExpectedDeliveryDate)
            .IsRequired();

        builder.HasMany(co => co.LineItems)
            .WithOne(li => li.ConfirmedOrder)
            .HasForeignKey(li => li.ConfirmedOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.Company)
            .WithMany(c => c.ConfirmedOrders)
            .HasForeignKey(co => co.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(co => new { co.CompanyId, co.OrderNumber }).IsUnique();
    }
}
