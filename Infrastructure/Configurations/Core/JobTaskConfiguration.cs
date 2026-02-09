using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core;

public class JobTaskConfiguration : IEntityTypeConfiguration<JobTask>
{
    public void Configure(EntityTypeBuilder<JobTask> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.JobTaskKey, x.CompanyId })
            .IsUnique();

        builder.Property(x => x.JobTaskKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.IsArchived)
            .IsRequired();

        // builder.Property(x => x.CronExpression)
        //     .IsRequired()
        //     .HasMaxLength(100);

        builder.HasMany(x => x.Jobs)
            .WithOne(j => j.JobTask)
            .HasForeignKey(j => j.JobTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Company)
            .WithMany(j => j.JobTasks)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
