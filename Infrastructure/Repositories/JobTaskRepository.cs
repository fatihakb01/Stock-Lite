using Domain.Entities.Core;
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
}
