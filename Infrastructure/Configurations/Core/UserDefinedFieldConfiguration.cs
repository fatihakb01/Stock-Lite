using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core;

public class UserDefinedFieldConfiguration : IEntityTypeConfiguration<UserDefinedField>
{
    public void Configure(EntityTypeBuilder<UserDefinedField> builder)
    {
        builder.HasKey(u => new { u.ProductId, u.Key });

        builder.HasOne(u => u.Product)
               .WithMany(p => p.UserDefinedFields)
               .HasForeignKey(u => u.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
