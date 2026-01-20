using System.Data;
using System.Globalization;
using CsvHelper;
using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Context;
using Infrastructure.Services.Scheduler.Imports.Records;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Scheduler.Imports.Handlers;

public class ProductImportHandler(DataContext context) : IImportHandler
{
    private readonly DataContext _context = context;
    public ImportDataType DataType => ImportDataType.Products;

    public async Task ImportAsync(Stream file, Guid companyId, CancellationToken ct)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        using var dataTable = new DataTable();
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Price", typeof(decimal));
        dataTable.Columns.Add("Description", typeof(string));
        dataTable.Columns.Add("IsActive", typeof(bool));
        dataTable.Columns.Add("LeadTimeDays", typeof(int));
        dataTable.Columns.Add("MinimumOrderQuantity", typeof(int));
        dataTable.Columns.Add("OrderIncrementQuantity", typeof(int));
        dataTable.Columns.Add("SafetyStock", typeof(int));
        dataTable.Columns.Add("StockAdjustment", typeof(int));

        foreach (var r in csv.GetRecords<ProductRecord>())
        {
            dataTable.Rows.Add(
                companyId,
                r.SKU,
                r.Name,
                r.Price,
                r.Description ?? "",
                r.IsActive ?? true,
                r.LeadTimeDays,
                r.MinimumOrderQuantity,
                r.OrderIncrementQuantity,
                r.SafetyStock,
                r.StockAdjustment
            );
        }

        using var connection = (SqlConnection)_context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = "dbo.StagingProducts",
            BatchSize = 50_000
        };

        await bulk.WriteToServerAsync(dataTable, ct);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.ProcessProducts @CompanyId",
            new SqlParameter("@CompanyId", companyId)
        );
    }
}
