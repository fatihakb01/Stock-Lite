using Domain.Enums;

namespace Domain.Services.Scheduler.Calculations;

public interface IForecastCalculation
{
    Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        Period? periodType,
        CancellationToken ct
    );
}
