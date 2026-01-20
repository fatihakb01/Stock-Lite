using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class JobTaskRepository(DataContext context) 
    : Repository<JobTask>(context), IJobTaskRepository
{
    private readonly DataContext _context = context;

    public async Task<JobTask?> GetJobAndExecutionsAsync(Guid jobTaskId, CancellationToken ct)
    {
        return await _context.JobTasks
            .Include(x => x.Jobs)
                .ThenInclude(x => x.JobExecutions
                    .OrderByDescending(x => x.StartedAt)
                    .Take(30)
                )
                .SingleOrDefaultAsync(x => x.Id == jobTaskId, ct);
    }
}
