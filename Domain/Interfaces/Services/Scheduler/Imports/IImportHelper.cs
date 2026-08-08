using Domain.Entities.Helper;
using Domain.Enums;
using System.Data;

namespace Domain.Interfaces.Services.Scheduler.Imports
{
    public interface IImportHelper
    {
        Task<ImportResult> ProcessImportAsync(
            ImportDataType dataType,
            Guid companyId,
            DataTable dataTable,
            CancellationToken ct
        );
    }
}
