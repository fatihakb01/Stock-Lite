using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ScheduledJobRepository(DataContext context)
    : Repository<ScheduledJob>(context), IScheduledJobRepository
{
    private readonly DataContext _context = context;

    public async Task<ScheduledJob?> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, CancellationToken ct)
    {
        return await _context.ScheduledJobs
            .Include(x => x.JobExecutions
                .OrderByDescending(x => x.StartedAt)
                .Take(30)
            )
            .SingleOrDefaultAsync(x => x.Id == scheduledJobId, ct);
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
