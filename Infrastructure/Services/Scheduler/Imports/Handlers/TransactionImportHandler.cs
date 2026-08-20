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

public class TransactionImportHandler(
    IImportHelper importHelper,
    IBaseCacheService<Transaction> cacheService,
    IProductNotificationService notificationService
) : BaseImportHandler<TransactionRecord>(importHelper), IImportHandler
{
    private readonly IBaseCacheService<Transaction> _cacheService = cacheService;
    private readonly IProductNotificationService _notificationService = notificationService;
    public override ImportDataType DataType => ImportDataType.Transactions;

    protected override DataTable CreateDataTable()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Version", typeof(int));
        dataTable.Columns.Add("ImportTimeStamp", typeof(DateTime));
        dataTable.Columns.Add("Id", typeof(Guid));
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("TransactionId", typeof(string));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("TransactionDate", typeof(DateTime));
        dataTable.Columns.Add("PaymentMethod", typeof(string));
        dataTable.Columns.Add("Notes", typeof(string));
        dataTable.Columns.Add("Quantity", typeof(int));
        return dataTable;
    }

    protected override void PopulateDataRow(DataTable dataTable, TransactionRecord record, Guid companyId, DateTime now)
    {
        dataTable.Rows.Add(
            0,
            now,
            Guid.NewGuid(),
            companyId,
            record.TransactionId,
            record.SKU,
            record.TransactionDate.Date,
            record.PaymentMethod ?? "",
            record.Notes ?? "",
            record.Quantity
        );
    }

    protected override async Task OnAfterImportAsync(ImportResult result, Guid companyId, CancellationToken ct)
    {
        // Remove transaction cache after import
        if (result.HasAffectedIds && result.AffectedIds.Count > 0)
        {
            await _cacheService.RemoveAsync(_cacheService.GetKey(companyId, TransactionKey.Transactions), ct);

            var tasks = result.AffectedIds.Select(productId =>
                _cacheService.RemoveAsync(_cacheService.GetKey(productId, TransactionKey.Transaction), ct));

            await Task.WhenAll(tasks);

            await _notificationService.NotifyProductTransactionsUpdated(result.AffectedIds, ct);
        }
    }
}