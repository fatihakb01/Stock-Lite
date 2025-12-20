using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(s => s.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.QuantityOnOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ReorderPoint)
            .IsRequired()
            .HasDefaultValue(0);

        builder.ToTable(p => {
            p.HasCheckConstraint("CK_StockLevel_QuantityOnHand", "[QuantityOnHand] >= 0");
            p.HasCheckConstraint("CK_StockLevel_QuantityOnOrder", "[QuantityOnOrder] >= 0");
            p.HasCheckConstraint("CK_StockLevel_ReorderPoint", "[ReorderPoint] >= 0");
        });
    }
}
