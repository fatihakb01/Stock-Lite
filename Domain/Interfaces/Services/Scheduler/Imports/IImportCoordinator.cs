using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler.Imports;

public interface IImportCoordinator
{
    Task RunAsync(
        ImportDataType type,
        string source,
        Guid companyId,
        CancellationToken ct
    );
}
