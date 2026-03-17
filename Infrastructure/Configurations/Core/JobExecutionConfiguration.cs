using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core;

public class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.TriggerType)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("nvarchar(MAX)");

        builder.HasIndex(x => new { x.ScheduledJobId, x.Status });
        builder.HasIndex(x => new { x.ScheduledJobId, x.StartedAt });

        builder.HasOne(x => x.ScheduledJob)
            .WithMany(j => j.JobExecutions)
            .HasForeignKey(j => j.ScheduledJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.JobTaskExecution)
            .WithMany(j => j.JobExecutions)
            .HasForeignKey(j => j.JobTaskExecutionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
