using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ConfirmedOrderAggregatesConfiguration : IEntityTypeConfiguration<ConfirmedOrderAggregate>
{
    public void Configure(EntityTypeBuilder<ConfirmedOrderAggregate> builder)
    {
        builder.HasKey(co => new { co.ProductId, co.PeriodType, co.PeriodStartDate });

        builder.Property(co => co.ProductId)
            .IsRequired();

        builder.Property(co => co.PeriodType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(co => co.PeriodStartDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(co => co.PeriodEndDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(co => co.TotalConfirmedQuantity)
            .IsRequired();

        builder.Property(co => co.TotalConfirmedRevenue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(co =>
        {
            co.HasCheckConstraint("CK_ConfirmedOrderAggregate_Total_Quantity", "[TotalConfirmedQuantity] >= 0");
            co.HasCheckConstraint("CK_ConfirmedOrderAggregate_Period_Start_End_Date", "[PeriodStartDate] <= [PeriodEndDate]");
        });

        builder.HasOne(co => co.Product)
            .WithMany(p => p.ConfirmedOrderAggregates)
            .HasForeignKey(co => co.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
