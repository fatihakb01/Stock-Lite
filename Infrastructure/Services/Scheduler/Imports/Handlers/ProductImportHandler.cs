using Domain.Entities.Helper;
using Domain.Enums;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports.Records;
using System.Data;

namespace Infrastructure.Services.Scheduler.Imports.Handlers;

public class ProductImportHandler(
    IImportHelper importHelper,
    IProductCacheService cacheService
) : BaseImportHandler<ProductRecord>(importHelper), IImportHandler
{
    private readonly IProductCacheService _cacheService = cacheService;
    public override ImportDataType DataType => ImportDataType.Products;

    protected override DataTable CreateDataTable()
    {
        var dataTable = new DataTable();
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
        return dataTable;
    }

    protected override void PopulateDataRow(DataTable dataTable, ProductRecord record, Guid companyId, DateTime now)
    {
        dataTable.Rows.Add(
            0,
            now,
            Guid.NewGuid(),
            companyId,
            record.SKU,
            record.Name,
            record.Price,
            record.Description ?? "",
            record.IsActive ?? true,
            now.Date,
            record.LeadTimeDays,
            record.MinimumOrderQuantity,
            record.OrderIncrementQuantity,
            record.SafetyStock,
            record.StockAdjustment
        );
    }

    protected override async Task OnAfterImportAsync(ImportResult result, Guid companyId, CancellationToken ct)
    {
        if (result.HasAffectedIds && result.AffectedIds.Count > 0)
        {
            await _cacheService.RemoveProductKeys(result.AffectedIds, companyId, ct);
        }
    }
}
