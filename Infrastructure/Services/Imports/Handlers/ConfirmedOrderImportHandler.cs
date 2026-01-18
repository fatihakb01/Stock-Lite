using System.Data;
using System.Globalization;
using CsvHelper;
using Domain.Enums;
using Domain.Services.Scheduler.Imports;
using Infrastructure.Context;
using Infrastructure.Services.Imports.Records;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Imports.Handlers;

public class ConfirmedOrderImportHandler(DataContext context) : IImportHandler
{
    private readonly DataContext _context = context;
    public ImportDataType DataType => ImportDataType.ConfirmedOrders;

    public async Task ImportAsync(Stream file, Guid companyId, CancellationToken ct)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        using var dataTable = new DataTable();
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("OrderNumber", typeof(string));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("OrderDate", typeof(DateTime));
        dataTable.Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
        dataTable.Columns.Add("ReceivedDate", typeof(DateTime));
        dataTable.Columns.Add("Quantity", typeof(int));
        dataTable.Columns.Add("SoldQuantity", typeof(int));

        foreach (var r in csv.GetRecords<ConfirmedOrderRecord>())
        {
            dataTable.Rows.Add(
                companyId,
                r.OrderNumber,
                r.SKU,
                r.OrderDate,
                r.ExpectedDeliveryDate,
                r.ReceivedDate ?? null,
                r.Quantity,
                r.SoldQuantity
            );
        }

        using var connection = (SqlConnection)_context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = "dbo.StagingConfirmedOrders",
            BatchSize = 50_000
        };

        await bulk.WriteToServerAsync(dataTable, ct);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.ProcessConfirmedOrders @CompanyId",
            new SqlParameter("@CompanyId", companyId)
        );
    }
}
