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

public class UserDefinedFieldImportHandler(DataContext context) : IImportHandler
{
    private readonly DataContext _context = context;
    public ImportDataType DataType => ImportDataType.UserDefinedFields;

    public async Task ImportAsync(Stream file, Guid companyId, CancellationToken ct)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        });

        using var dataTable = new DataTable();
        dataTable.Columns.Add("CompanyId", typeof(Guid));
        dataTable.Columns.Add("SKU", typeof(string));
        dataTable.Columns.Add("Key", typeof(string));
        dataTable.Columns.Add("Value", typeof(string));

        foreach (var r in csv.GetRecords<UserDefinedFieldRecord>())
        {
            dataTable.Rows.Add(
                companyId,
                r.SKU,
                r.Key,
                r.Value ?? ""
            );
        }

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            using var bulk = new SqlBulkCopy(connection)
            {
                DestinationTableName = "dbo.StagingUserDefinedFields",
                BatchSize = 50_000
            };

            await bulk.WriteToServerAsync(dataTable, ct);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.ProcessUserDefinedFields @CompanyId",
                new SqlParameter("@CompanyId", companyId)
            );
        }
        finally
        {
            if (shouldClose && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }
}
