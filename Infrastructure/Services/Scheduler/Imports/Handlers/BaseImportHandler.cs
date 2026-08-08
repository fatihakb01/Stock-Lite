using CsvHelper;
using Domain.Entities.Helper;
using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Imports;
using System.Data;
using System.Globalization;

namespace Infrastructure.Services.Scheduler.Imports.Handlers
{
    public abstract class BaseImportHandler<T>(IImportHelper importHelper) : IImportHandler where T : class
    {
        protected readonly IImportHelper _importHelper = importHelper;
        public abstract ImportDataType DataType { get; }

        public virtual async Task ImportAsync(Stream file, Guid companyId, CancellationToken ct)
        {
            var dataTable = await ParseCsvToDataTableAsync(file, companyId);

            if (dataTable.Rows.Count == 0)
                return;

            var result = await _importHelper.ProcessImportAsync(DataType, companyId, dataTable, ct);

            await OnAfterImportAsync(result, companyId, ct);
        }

        protected abstract DataTable CreateDataTable();
        protected abstract void PopulateDataRow(DataTable dataTable, T record, Guid companyId, DateTime now);

        protected virtual Task OnAfterImportAsync(ImportResult result, Guid companyId, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private async Task<DataTable> ParseCsvToDataTableAsync(Stream file, Guid companyId)
        {
            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });

            var dataTable = CreateDataTable();
            var now = DateTime.UtcNow;

            var records = csv.GetRecords<T>();

            foreach (var record in records)
            {
                PopulateDataRow(dataTable, record, companyId, now);
            }

            return dataTable;
        }
    }
}
