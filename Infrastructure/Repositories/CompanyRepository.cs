using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CompanyRepository(DataContext context)
        : Repository<Company>(context), ICompanyRepository
    {
        private readonly DataContext _context = context;
        public async Task<List<Guid>> GetCompanyIds(CancellationToken ct)
        {
            return await _context.Companies.Select(c => c.Id).ToListAsync(ct);
        }
    }
}
