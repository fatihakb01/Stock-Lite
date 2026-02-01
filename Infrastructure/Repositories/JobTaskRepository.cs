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
            .Where(e => e.ScheduledJob.JobTaskId == jobTaskId &&
                e.ScheduledJob.JobTask.CompanyId == companyId
            )
            .OrderByDescending(e => e.StartedAt)
            .Take(30)
            .Include(e => e.ScheduledJob)
                .ThenInclude(j => j.JobTask)
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
}
