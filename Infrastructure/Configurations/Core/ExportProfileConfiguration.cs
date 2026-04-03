using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core
{
    public class ExportProfileConfiguration : IEntityTypeConfiguration<ExportProfile>
    {
        public void Configure(EntityTypeBuilder<ExportProfile> builder)
        {
            builder.HasKey(ex => ex.Id);

            builder.HasIndex(ex => new { ex.CompanyId, ex.Id });

            builder.HasIndex(ex => new { ex.CompanyId, ex.Name });

            builder.Property(ex => ex.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ex => ex.Description)
                .HasMaxLength(200);

            builder.Property(ex => ex.CustomSource)
                .HasMaxLength(100);

            builder.Property(ex => ex.ExportFileType)
                .IsRequired();

            builder.Property(ex => ex.CreatedBy)
                .IsRequired();

            builder.ToTable(ex =>
            {
                ex.HasCheckConstraint(
                    "CK_ExportProfile_Custom_Source_Or_Export_Type",
                    @"
                    ([CustomSource] IS NULL AND [ExportType] IS NOT NULL)
                    OR ([CustomSource] IS NOT NULL AND [ExportType] IS NULL)
                    "
                );

                ex.HasCheckConstraint(
                    "CK_ExportProfile_Custom_Source_And_Type",
                    @"
                    ([CustomSource] IS NULL AND [CustomType] IS NULL)
                    OR ([CustomSource] IS NOT NULL AND [CustomType] IS NOT NULL)
                    "
                );
            });

            builder.HasOne(b => b.Company)
                .WithMany(ex => ex.ExportProfiles)
                .HasForeignKey(ex => ex.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
