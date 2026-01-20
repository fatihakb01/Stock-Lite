using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Core;

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.JobType)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.HasIndex(x => new { x.JobTaskId, x.ExecutionOrder })
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ScheduledJob_Target_Only_For_Custom",
                @"
                ([JobType] = 2 AND [Target] IS NOT NULL)
                OR ([JobType] <> 2 AND [Target] IS NULL)
                "
            );
            t.HasCheckConstraint(
                "CK_ScheduledJob_Import_Requires_DataType_And_Source",
                @"
                (JobType <> 0) OR 
                (ImportDataType IS NOT NULL AND Source IS NOT NULL)
                "
            );
            t.HasCheckConstraint(
                "CK_ScheduledJob_StandardCalculation_Requires_CalculationType",
                @"
                ([JobType] <> 1) OR ([CalculationType] IS NOT NULL)
                "
            );
        });

        builder.HasOne(x => x.JobTask)
            .WithMany(j => j.Jobs)
            .HasForeignKey(j => j.JobTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.JobExecutions)
            .WithOne(j => j.ScheduledJob)
            .HasForeignKey(j => j.ScheduledJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
