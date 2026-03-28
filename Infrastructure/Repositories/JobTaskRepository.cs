using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class JobTaskRepository(DataContext context) 
    : Repository<JobTask>(context), IJobTaskRepository 
{
    private readonly DataContext _context = context;

    public async Task<IReadOnlyList<JobExecution>> GetJobsAndExecutionsAsync(
        Guid jobTaskId,
        Guid companyId,
        CancellationToken ct)
    {
        return await _context.JobExecutions
            .Where(e => 
                e.ScheduledJob.JobTaskId == jobTaskId &&
                e.ScheduledJob.JobTask.CompanyId == companyId
            )
            .OrderByDescending(e => e.StartedAt)
            .Take(30)
            .Include(e => e.ScheduledJob)
            .ThenInclude(j => j.JobTask)
            .ToListAsync(ct);
    }

    public async Task<JobTask?> GetNextDueTaskAsync(Guid companyId, DateTime now, CancellationToken ct)
    {
        return await _context.JobTasks
            .Where(x =>
                x.CompanyId == companyId &&
                !x.IsArchived &&
                !x.AllowManualTriggerOnly &&
                (
                    x.Result != JobExecutionStatus.Running ||
                    x.LastStartedAt < now.AddMinutes(-(x.RetryDelayMinutes ?? 1))
                ) &&
                x.Retries < x.MaxRetries &&
                x.NextRunAt != null &&
                x.NextRunAt <= now)
            .OrderBy(x => x.NextRunAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> HasExhaustedTaskAsync(Guid companyId, CancellationToken ct)
    {
        return await _context.JobTasks
            .Where(x =>
                x.CompanyId == companyId &&
                x.Result == JobExecutionStatus.Failed &&
                x.Retries >= x.MaxRetries)
            .AnyAsync(ct);
    }

    public async Task<bool> TryLockTaskAsync(Guid jobTaskId, DateTime now)
    {
        var affected = await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE JobTasks
            SET Result = 1,
                LastStartedAt = {0}
            WHERE Id = {1}
            AND Result <> 1
            """,
            now,
            jobTaskId
        );

        return affected == 1;
    }

    public async Task CleanJobTask(Guid jobTaskId, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE JobTasks
            SET Retries = 0,
                Result = 0
            WHERE Id = {0}
            """,
            jobTaskId
        );

        await _context.SaveChangesAsync(ct);
    }

    public async Task ResetScheduler(Guid companyId, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
        DECLARE @ResetJobTaskIds TABLE (Id UNIQUEIDENTIFIER);

        WITH AvailableJobTasks AS (
            SELECT COUNT(*) AS Total
            FROM JobTasks
            WHERE CompanyId = {0}
            AND IsArchived = 0
        ),
        SucceededJobTasks AS (
            SELECT COUNT(*) AS Total
            FROM JobTasks
            WHERE CompanyId = {0}
            AND IsArchived = 0
            AND Result = 2
        )
        UPDATE JobTasks
        SET Result = 0
        OUTPUT inserted.Id INTO @ResetJobTaskIds
        WHERE CompanyId = {0}
        AND IsArchived = 0
        AND (SELECT Total FROM AvailableJobTasks) = (SELECT Total FROM SucceededJobTasks);

        IF EXISTS (SELECT 1 FROM @ResetJobTaskIds)
        BEGIN
            UPDATE ScheduledJobs
            SET Result = 0
            WHERE JobTaskId IN (SELECT Id FROM @ResetJobTaskIds)
            AND IsArchived = 0;
        END
        """,
            companyId
        );

        await _context.SaveChangesAsync(ct);
    }
}
