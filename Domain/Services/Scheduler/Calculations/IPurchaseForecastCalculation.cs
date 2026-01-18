namespace Domain.Services.Scheduler.Calculations;

public interface IPurchaseForecastCalculation
{
    Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        CancellationToken ct
    );
}
