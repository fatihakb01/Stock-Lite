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

    public async Task<List<JobTask>> GetDueTasksAsync(DateTime now, CancellationToken ct)
    {
        return await _context.JobTasks
            .Where(x =>
                !x.IsArchived &&
                (
                    !x.IsRunning ||
                    x.LastStartedAt < now.AddMinutes(-x.RetryDelayMinutes)
                ) &&
                x.Retries < x.MaxRetries &&
                x.NextRunAt != null &&
                x.NextRunAt <= now)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, JobRunResult>> GetScheduledJobResultByJobTaskAsync(
        Guid jobTaskId, CancellationToken ct)
    {
        return await _context.ScheduledJobs
            .Where(x => x.JobTaskId == jobTaskId)
            .GroupBy(x => x.Id)
            .Select(g => g
                .Select(x => new
                {
                    x.Id,
                    x.Result
                })
                .First()
            )
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Result,
                ct
            );
    }

    public async Task ResetScheduledJobResultByJobTaskAsync(Guid jobTaskId, CancellationToken ct)
    {
        var jobs = _context.ScheduledJobs
            .Where(x => x.JobTaskId == jobTaskId)
            .ToList();

        var succeededJobs = _context.ScheduledJobs
            .Where(x => x.JobTaskId == jobTaskId && x.Result == JobRunResult.Succeeded)
            .ToList();

        if (jobs.Count != succeededJobs.Count)
            return;

        foreach (var job in jobs)
        {
            job.Result = JobRunResult.Unknown;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ResetJobTaskResultByJobTaskAsync(Guid jobTaskId, CancellationToken ct)
    {
        var jobTasks = _context.JobTasks
            .Where(x => x.Id == jobTaskId)
            .ToList();

        var succeededJobTasks = _context.JobTasks
            .Where(x => x.Id == jobTaskId && x.Result == JobRunResult.Succeeded)
            .ToList();

        if (jobTasks.Count != succeededJobTasks.Count)
            return;

        foreach (var jobTask in jobTasks)
        {
            jobTask.Result = JobRunResult.Unknown;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> TryLockTaskAsync(Guid taskId, DateTime now)
    {
        var affected = await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE JobTasks
            SET IsRunning = 1,
                LastStartedAt = {0}
            WHERE Id = {1}
            AND IsRunning = 0
            """,
            now,
            taskId
        );

        return affected == 1;
    }

    public async Task CleanJobTask(Guid taskId, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            UPDATE JobTasks
            SET IsRunning = 0,
                Retries = 0,
                Result = 2
            WHERE Id = {0}
            """,
            taskId
        );

        await _context.SaveChangesAsync(ct);
    }
}
