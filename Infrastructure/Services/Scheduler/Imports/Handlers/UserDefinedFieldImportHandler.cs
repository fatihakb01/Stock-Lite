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

public class UserDefinedFieldImportHandler(
    IImportHelper importHelper,
    IBaseCacheService<UserDefinedField> cacheService,
    IProductNotificationService notificationService
) : BaseImportHandler<UserDefinedFieldRecord>(importHelper), IImportHandler
{
    private readonly IBaseCacheService<UserDefinedField> _cacheService = cacheService;
    private readonly IProductNotificationService _notificationService = notificationService;
    public override ImportDataType DataType => ImportDataType.UserDefinedFields;

    protected override DataTable CreateDataTable()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Version", typeof(int));
        dataTable.Columns.Add("ImportTimeStamp", typeof(DateTime));
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("Key", typeof(string));
        dataTable.Columns.Add("Value", typeof(string));
        return dataTable;
    }

    protected override void PopulateDataRow(DataTable dataTable, UserDefinedFieldRecord record, Guid companyId, DateTime now)
    {
        dataTable.Rows.Add(
            0,
            now,
            companyId,
            record.SKU,
            record.Key,
            record.Value ?? ""
        );
    }

    protected override async Task OnAfterImportAsync(ImportResult result, Guid companyId, CancellationToken ct)
    {
        // Remove user defined field cache after import
        if (result.HasAffectedIds && result.AffectedIds.Count > 0)
        {
            var tasks = result.AffectedIds.Select(productId =>
                _cacheService.RemoveAsync(_cacheService.GetKey(productId, UserDefinedFieldKey.UserDefinedField), ct));

            await Task.WhenAll(tasks);

            await _notificationService.NotifyProductUserDefinedFieldsUpdated(result.AffectedIds, ct);
        }
    }
}
