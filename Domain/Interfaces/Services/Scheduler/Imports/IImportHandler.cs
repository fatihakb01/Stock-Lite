using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler.Imports;

public interface IImportHandler
{
    public ImportDataType DataType { get; }
    public Task ImportAsync(Stream file, Guid companyId, CancellationToken ct);
}
