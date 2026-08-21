using Domain.Entities.Core;
using Domain.Entities.Helper;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Imports;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.Scheduler.Imports.Records;
using System.Data;

namespace Infrastructure.Services.Scheduler.Imports.Handlers;

public class ConfirmedOrderImportHandler(
    IImportHelper importHelper,
    IBaseCacheService<ConfirmedOrder> cacheService,
    IProductNotificationService notificationService
) : BaseImportHandler<ConfirmedOrderRecord>(importHelper), IImportHandler
{
    private readonly IBaseCacheService<ConfirmedOrder> _cacheService = cacheService;
    private readonly IProductNotificationService _notificationService = notificationService;
    public override ImportDataType DataType => ImportDataType.ConfirmedOrders;

    protected override DataTable CreateDataTable()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Version", typeof(int));
        dataTable.Columns.Add("ImportTimeStamp", typeof(DateTime));
        dataTable.Columns.Add("Id", typeof(Guid));
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("OrderNumber", typeof(string));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("OrderDate", typeof(DateTime));
        dataTable.Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
        dataTable.Columns.Add("ReceivedDate", typeof(DateTime));
        dataTable.Columns.Add("Quantity", typeof(int));
        dataTable.Columns.Add("SoldQuantity", typeof(int));
        dataTable.Columns.Add("UnitCost", typeof(decimal));
        return dataTable;
    }

    protected override void PopulateDataRow(DataTable dataTable, ConfirmedOrderRecord record, Guid companyId, DateTime now)
    {
        dataTable.Rows.Add(
            0,
            now,
            Guid.NewGuid(),
            companyId,
            record.OrderNumber,
            record.SKU,
            record.OrderDate.Date,
            record.ExpectedDeliveryDate.Date,
            record.ReceivedDate?.Date ?? null,
            record.Quantity,
            record.SoldQuantity,
            record.UnitCost
        );
    }

    protected override async Task OnAfterImportAsync(ImportResult result, Guid companyId, CancellationToken ct)
    {
        // Remove transaction cache after import
        if (result.HasAffectedIds && result.AffectedIds.Count > 0)
        {
            await _cacheService.RemoveAsync(_cacheService.GetKey(companyId, ConfirmedOrderKey.ConfirmedOrders), ct);

            var tasks = result.AffectedIds.Select(productId =>
                _cacheService.RemoveAsync(_cacheService.GetKey(productId, ConfirmedOrderKey.ConfirmedOrder), ct));

            await Task.WhenAll(tasks);

            await _notificationService.NotifyProductConfirmedOrdersUpdated(result.AffectedIds, ct);
        }
    }
}
