using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.ToTable(p =>
            p.HasCheckConstraint("CK_Transaction_TransactionDate", "[TransactionDate] <= SYSUTCDATETIME()")
        );

        builder.Property(t => t.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(t => t.Notes)
            .HasMaxLength(200);

        builder.HasOne(t => t.Customer)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.LineItems)
            .WithOne(l => l.Transaction)
            .HasForeignKey(l => l.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
