using CsvHelper;
using Domain.Enums;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports.Records;
using System.Data;
using System.Globalization;

namespace Infrastructure.Services.Scheduler.Imports.Handlers;

public class ProductImportHandler(
    IImportHelper importHelper,
    IProductCacheService cacheService
) : IImportHandler
{
    private readonly IProductCacheService _cacheService = cacheService;
    public ImportDataType DataType => ImportDataType.Products;

    public async Task ImportAsync(Stream file, Guid companyId, CancellationToken ct)
    {
        var dataTable = await ParseCsvToDataTableAsync(file, companyId);

        var result = await importHelper.ProcessImportAsync(DataType, companyId, dataTable, ct);

        if (result.HasAffectedIds && result.AffectedIds.Count > 0)
        {
            await _cacheService.RemoveProductKeys(result.AffectedIds, companyId, ct);
        }
    }

    private static async Task<DataTable> ParseCsvToDataTableAsync(Stream file, Guid companyId)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        });

        var dataTable = new DataTable();
        var now = DateTime.UtcNow;

        // Define columns
        dataTable.Columns.Add("Version", typeof(int));
        dataTable.Columns.Add("ImportTimeStamp", typeof(DateTime));
        dataTable.Columns.Add("ProductId", typeof(Guid));
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Price", typeof(decimal));
        dataTable.Columns.Add("Description", typeof(string));
        dataTable.Columns.Add("IsActive", typeof(bool));
        dataTable.Columns.Add("CreatedAt", typeof(DateTime));
        dataTable.Columns.Add("LeadTimeDays", typeof(int));
        dataTable.Columns.Add("MinimumOrderQuantity", typeof(int));
        dataTable.Columns.Add("OrderIncrementQuantity", typeof(int));
        dataTable.Columns.Add("SafetyStock", typeof(int));
        dataTable.Columns.Add("StockAdjustment", typeof(int));

        foreach (var r in csv.GetRecords<ProductRecord>())
        {
            dataTable.Rows.Add(
                0,
                now,
                Guid.NewGuid(),
                companyId,
                r.SKU,
                r.Name,
                r.Price,
                r.Description ?? "",
                r.IsActive ?? true,
                now.Date,
                r.LeadTimeDays,
                r.MinimumOrderQuantity,
                r.OrderIncrementQuantity,
                r.SafetyStock,
                r.StockAdjustment
            );
        }

        return dataTable;
    }
}
