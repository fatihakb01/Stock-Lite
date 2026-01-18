namespace Domain.Services.Scheduler.Calculations;

public interface IStockLevelCalculation
{
    Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        CancellationToken ct
    );
}
