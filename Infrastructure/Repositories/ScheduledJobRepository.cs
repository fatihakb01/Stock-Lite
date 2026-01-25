using Domain.Entities.Core;
using Domain.Enums;
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

    public async Task<Dictionary<Guid, JobExecutionStatus>> GetLatestExecutionStatusByJobTaskAsync(
        Guid jobTaskId, CancellationToken ct)
    {
        return await _context.JobExecutions
            .Where(x => x.ScheduledJob.JobTaskId == jobTaskId)
            .GroupBy(x => x.ScheduledJobId)
            .Select(g => g
                .OrderByDescending(x => x.StartedAt)
                .Select(x => new
                {
                    x.ScheduledJobId,
                    x.Status
                })
                .First()
            )
            .ToDictionaryAsync(
                x => x.ScheduledJobId,
                x => x.Status,
                ct
            );
    }
}
