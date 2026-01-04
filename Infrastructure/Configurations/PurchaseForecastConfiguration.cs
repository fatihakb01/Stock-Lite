using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PurchaseForecastConfiguration : IEntityTypeConfiguration<PurchaseForecast>
{
    public void Configure(EntityTypeBuilder<PurchaseForecast> builder)
    {
        builder.HasKey(x => new {x.ProductId, x.WeekStartDate});

        builder.Property(x => x.WeekStartDate)
            .IsRequired();

        builder.Property(x => x.WeekEndDate)
            .IsRequired();

        builder.Property(x => x.WeekIndex)
            .IsRequired();

        builder.Property(x => x.ForecastQuantity)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(x => x.QuantityCommitted)
            .IsRequired();

        builder.Property(x => x.QuantityOnOrder)
            .IsRequired();

        builder.Property(x => x.ProjectedStock)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(x => x.CalculatedAt)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.PurchaseForecasts)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(p => {
            p.HasCheckConstraint("CK_PurchaseForecast_ForecastQuantity", "[ForecastQuantity] >= 0");
            p.HasCheckConstraint("CK_PurchaseForecast_QuantityCommitted", "[QuantityCommitted] >= 0");
            p.HasCheckConstraint("CK_PurchaseForecast_QuantityOnOrder", "[QuantityOnOrder] >= 0");
            p.HasCheckConstraint("CK_PurchaseForecast_RecommendedOrderQuantity", "[RecommendedOrderQuantity] >= 0");
        });
    }
}
