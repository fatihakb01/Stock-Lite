using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TransactionLineItemConfiguration : IEntityTypeConfiguration<TransactionLineItem>
{
    public void Configure(EntityTypeBuilder<TransactionLineItem> builder)
    {
        builder.HasKey(tl => tl.Id);

        builder.Property(tl => tl.Quantity)
            .IsRequired();

        builder.Property(tl => tl.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(tl =>
            tl.HasCheckConstraint("CK_TransactionLineItem_UnitPrice", "[UnitPrice] >= 0")
        );

        builder.HasOne(tl => tl.Transaction)
            .WithMany(t => t.LineItems)
            .HasForeignKey(tl => tl.TransactionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(tl => tl.Product)
            .WithMany(p => p.TransactionLineItems)
            .HasForeignKey(tl => tl.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(tl => new { tl.TransactionId, tl.ProductId }).IsUnique();
    }
}
