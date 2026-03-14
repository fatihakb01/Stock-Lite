using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;

namespace Infrastructure.Repositories
{
    public class JobTaskExecutionRepository(DataContext context) 
        : Repository<JobTaskExecution>(context), IJobTaskExecutionRepository
    {

    }
}
