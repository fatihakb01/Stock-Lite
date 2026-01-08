using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> builder)
    {
        builder.HasKey(f => new { f.ProductId, f.PeriodType, f.PeriodStartDate });

        builder.Property(f => f.ProductId)
            .IsRequired();

        builder.Property(f => f.PeriodType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(f => f.PeriodStartDate)
            .IsRequired();

        builder.Property(f => f.PeriodEndDate)
            .IsRequired();

        builder.Property(f => f.PredictedQuantity)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(f => f.PredictedRevenue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.Confidence)
            .IsRequired()
            .HasColumnType("decimal(3,2)");

        builder.ToTable(f =>
        {
            f.HasCheckConstraint("CK_Forecast_Confidence", "[Confidence] > 0 AND [Confidence] < 1");
            f.HasCheckConstraint("CK_Forecast_Period_Start_End_Date", "[PeriodStartDate] <= [PeriodEndDate]");
        });

        builder.HasOne(f => f.Product)
            .WithMany(p => p.Forecasts)
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
