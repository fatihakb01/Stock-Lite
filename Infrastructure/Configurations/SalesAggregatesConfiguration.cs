using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SalesAggregateConfiguration : IEntityTypeConfiguration<SalesAggregate>
{
    public void Configure(EntityTypeBuilder<SalesAggregate> builder)
    {
        builder.HasKey(s => new { s.ProductId, s.PeriodType, s.PeriodStartDate });

        builder.Property(sa => sa.ProductId)
            .IsRequired();

        builder.Property(sa => sa.PeriodType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(sa => sa.PeriodStartDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(sa => sa.PeriodEndDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(sa => sa.TotalQuantity)
            .IsRequired();

        builder.Property(sa => sa.TotalRevenue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(sa =>
        {
            sa.HasCheckConstraint("CK_SalesAggregate_Total_Quantity", "[TotalQuantity] >= 0");
            sa.HasCheckConstraint("CK_SalesAggregate_Period_Start_End_Date", "[PeriodStartDate] <= [PeriodEndDate]");
        });

        builder.HasOne(sa => sa.Product)
            .WithMany(p => p.SalesAggregates)
            .HasForeignKey(sa => sa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
