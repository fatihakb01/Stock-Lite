using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class JobExecutionRepository(DataContext context)
    : Repository<JobExecution>(context), IJobExecutionRepository
{
    private readonly DataContext _context = context;

    public async Task<IReadOnlyList<JobExecution>> GetRecentExecutionAsync(
        Guid companyId, 
        int days, 
        CancellationToken ct
    )
    {
        var since = DateTime.UtcNow.AddDays(-days);

        return await _context.JobExecutions
            .Include(x => x.ScheduledJob)
            .ThenInclude(x => x.JobTask)
            .Where(x => 
                x.ScheduledJob.JobTask.CompanyId == companyId &&
                x.StartedAt >= since
            )
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(ct);
    }
}
