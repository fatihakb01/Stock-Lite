using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Export
{
    public interface IExportService
    {
        Task ExportAsync(ExportProfile profile, Guid companyId, CancellationToken ct);
    }
}
