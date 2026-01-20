using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler.Calculations;

public interface IConfirmedOrderAggregateCalculation
{
    Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        Period? periodType,
        CancellationToken ct
    );
}
