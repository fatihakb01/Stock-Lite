using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Export.Records;

namespace Domain.Interfaces.Services.Export
{
    public interface IExportService
    {
        Task ExportToFileAsync(
            ExportProfile profile, 
            Guid companyId, 
            CancellationToken ct
        );
        Task<ExportResult> ExportToStreamAsync(
            ExportProfile profile,
            FileType fileType,
            Guid companyId, 
            CancellationToken ct
        );
    }
}
