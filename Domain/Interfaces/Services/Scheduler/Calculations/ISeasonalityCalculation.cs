namespace Domain.Interfaces.Services.Scheduler.Calculations;

public interface ISeasonalityCalculation
{
    Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        CancellationToken ct
    );
}
