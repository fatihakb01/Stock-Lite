using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(b => b.Address)
            .HasMaxLength(250);

        builder.HasMany(b => b.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Products)
            .WithOne(p => p.Company)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Customers)
            .WithOne(c => c.Company)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
