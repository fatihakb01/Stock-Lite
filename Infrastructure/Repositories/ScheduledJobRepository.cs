using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ScheduledJobRepository(DataContext context)
    : Repository<ScheduledJob>(context), IScheduledJobRepository
{
    private readonly DataContext _context = context;

    public async Task<IReadOnlyList<JobExecution>> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, Guid companyId, CancellationToken ct)
    {
        return await _context.JobExecutions
            .Where(e => e.ScheduledJobId == scheduledJobId &&
                e.ScheduledJob.JobTask.CompanyId == companyId
            )
            .OrderByDescending(e => e.StartedAt)
            .Take(30)
            .Include(e => e.ScheduledJob)
            .ThenInclude(e => e.JobTask)
            .ToListAsync(ct);
    }

    public async Task AddScheduledJobAsync(ScheduledJob job, CancellationToken ct)
    {
        var maxOrder = await _context.ScheduledJobs
            .Where(j => j.JobTaskId == job.JobTaskId)
            .MaxAsync(j => (int?)j.ExecutionOrder, ct) ?? -1;

        job.ExecutionOrder = maxOrder + 1;

        _context.ScheduledJobs.Add(job);
    }

    public async Task DeleteScheduledJobAsync(Guid jobId, CancellationToken ct)
    {
        var job = await _context.ScheduledJobs.FindAsync(jobId, ct);
        if (job == null) return;

        var jobTaskId = job.JobTaskId;
        _context.ScheduledJobs.Remove(job);

        var remaining = await _context.ScheduledJobs
            .Where(j => j.JobTaskId == jobTaskId)
            .OrderBy(j => j.ExecutionOrder)
            .ToListAsync(ct);

        for (var i = 0; i < remaining.Count; i++)
        {
            remaining[i].ExecutionOrder = i;
        }
    }

    public async Task ReorderScheduledJobAsync(List<Guid> orderedScheduledJobIds, Guid companyId, CancellationToken ct)
    {
        var jobs = await _context.ScheduledJobs
            .Where(j => 
                orderedScheduledJobIds.Contains(j.Id) && 
                j.JobTask.CompanyId == companyId
            )
            .ToListAsync(ct);

        if (jobs.Count != orderedScheduledJobIds.Count)
            throw new InvalidOperationException("Job mismatch");

        // Temporary solution to make sure jobexecution does not violate index (JobTaskId, ExecutionOrder)
        foreach (var job in jobs)
        {
            job.ExecutionOrder += 1000;
        }

        await _context.SaveChangesAsync(ct);

        // Update ExecutionOrder
        for (int i = 0; i < orderedScheduledJobIds.Count; i++)
        {
            var job = jobs.Single(j => j.Id == orderedScheduledJobIds[i]);
            job.ExecutionOrder = i;
        }
    }

    public async Task CleanScheduledJobs(Guid taskId, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE s
            SET s.Result = 0
            FROM ScheduledJobs s
            INNER JOIN JobTasks j ON j.Id = s.JobTaskId
            WHERE j.Id = {0}
            AND s.Result <> 0
            """,
            taskId
        );

        await _context.SaveChangesAsync(ct);
    }

    public async Task CleanScheduledJob(Guid jobId, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            DECLARE @jobTaskId UNIQUEIDENTIFIER;

            SELECT @jobTaskId = JobTaskId FROM ScheduledJobs WHERE Id = {0};

            UPDATE ScheduledJobs
            SET Result = 0
            WHERE Id = {0}
            AND Result <> 0;

            UPDATE JobTasks
            SET Retries = 0
            WHERE Id = @jobTaskId
            AND Retries <> 0;
            """,
            jobId
        );

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> TryLockTaskAsync(Guid jobId)
    {
        var affected = await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE ScheduledJobs
            SET Result = 1
            WHERE Id = {0}
            AND Result <> 1
            """,
            jobId
        );

        return affected == 1;
    }
}
