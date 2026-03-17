using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core
{
    public class JobTaskExecutionConfiguration : IEntityTypeConfiguration<JobTaskExecution>
    {
        public void Configure(EntityTypeBuilder<JobTaskExecution> builder)
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

            builder.HasIndex(x => new { x.JobTaskId, x.Status });
            builder.HasIndex(x => new { x.JobTaskId, x.StartedAt });

            builder.HasOne(x => x.JobTask)
                .WithMany(x => x.Executions)
                .HasForeignKey(x => x.JobTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.JobExecutions)
                .WithOne(x => x.JobTaskExecution)
                .HasForeignKey(x => x.JobTaskExecutionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
