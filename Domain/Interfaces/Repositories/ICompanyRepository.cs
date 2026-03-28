using Domain.Entities.Core;

namespace Domain.Interfaces.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<List<Guid>> GetCompanyIds(CancellationToken ct);
    }
}
