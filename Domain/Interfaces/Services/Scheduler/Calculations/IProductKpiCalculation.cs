namespace Domain.Interfaces.Services.Scheduler.Calculations;

public interface IProductKpiCalculation
{
    Task RunAsync(Guid companyId, CancellationToken ct);
}
