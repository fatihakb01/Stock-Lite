using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LegalName)
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.HasOne(c => c.Company)
            .WithMany(b => b.Customers)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Customer)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.CompanyId, c.DisplayName }).IsUnique();
        builder.HasIndex(c => new { c.CompanyId, c.LegalName }).IsUnique();
        builder.HasIndex(c => new { c.CompanyId, c.Email }).IsUnique();
    }
}
