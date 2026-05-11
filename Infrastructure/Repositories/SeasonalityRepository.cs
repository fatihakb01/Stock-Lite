using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

public class SeasonalityRepository
    : Repository<Seasonality>, ISeasonalityRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public SeasonalityRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
    }

    public async Task<IEnumerable<Seasonality>> GetSeasonalityByProductAsync(Guid productId)
    {
        DateTime endDate = DateTime.UtcNow.AddMonths(12);

        return await _context.Seasonality
            .Where(s =>
                s.ProductId == productId && 
                s.PeriodStartDate < endDate
            ).ToListAsync();
    }

    public async Task DeleteSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.DeletePastSeasonality", companyId, productIds);
    }

    public async Task InsertSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.InsertSeasonality", companyId, productIds);
    }

    public async Task UpdateSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.UpdateSeasonality", companyId, productIds);
    }
}
