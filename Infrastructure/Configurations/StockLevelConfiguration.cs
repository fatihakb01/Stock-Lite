using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.HasKey(s => s.ProductId);

        builder.Property(s => s.QuantityOnHand)
            .IsRequired();

        builder.Property(s => s.QuantityOnOrder)
            .IsRequired();

        builder.Property(s => s.QuantityCommitted)
            .IsRequired();

        builder.Property(s => s.NetStockPosition)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(s => s.ForecastQuantity)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(s => s.ProjectedStockAtLeadTime)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(s => s.ReorderPoint)
            .IsRequired();

        builder.Property(s => s.NeedsReorder)
            .IsRequired();

        builder.Property(s => s.RecommendedOrderQuantity)
            .IsRequired();

        builder.Property(s => s.LastCalculatedAt)
            .IsRequired();

        builder.ToTable(p => {
            p.HasCheckConstraint("CK_StockLevel_QuantityOnHand", "[QuantityOnHand] >= 0");
            p.HasCheckConstraint("CK_StockLevel_QuantityOnOrder", "[QuantityOnOrder] >= 0");
            p.HasCheckConstraint("CK_StockLevel_QuantityCommitted", "[QuantityCommitted] >= 0");
            p.HasCheckConstraint("CK_StockLevel_ForecastQuantity", "[ForecastQuantity] >= 0");
            p.HasCheckConstraint("CK_StockLevel_ReorderPoint", "[ReorderPoint] >= 0");
            p.HasCheckConstraint("CK_StockLevel_RecommendedOrderQuantity", "[RecommendedOrderQuantity] >= 0");
        });
    }
}
