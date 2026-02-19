using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Derived;

public class SeasonalityConfiguration : IEntityTypeConfiguration<Seasonality>
{
    public void Configure(EntityTypeBuilder<Seasonality> builder)
    {
        builder.HasKey(s => new { s.ProductId, s.PeriodStartDate });

        builder.Property(s => s.Factor)
            .IsRequired()
            .HasColumnType("decimal(4,2)");

        builder.ToTable(s =>
        {
            s.HasCheckConstraint("CK_Seasonality_Factor", "[Factor] >= 0 AND [Factor] <= 12");
        });

        builder.HasOne(s => s.Product)
               .WithMany(p => p.Seasonality)
               .HasForeignKey(s => s.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
