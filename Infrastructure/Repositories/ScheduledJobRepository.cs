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
