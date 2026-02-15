using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Derived;

public class ProductKpiConfiguration : IEntityTypeConfiguration<ProductKpi>
{
    public void Configure(EntityTypeBuilder<ProductKpi> builder)
    {
        builder.HasKey(x => x.CompanyId);

        builder.Property(x => x.TotalProductCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.BelowSafetyStockCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.RecommendedOrdersCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.WithoutStockAtLeadTimeCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.WithoutForecastCount)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
