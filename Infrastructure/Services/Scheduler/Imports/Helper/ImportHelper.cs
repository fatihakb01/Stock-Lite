using Domain.Entities.Helper;
using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using System.Data;

namespace Infrastructure.Services.Scheduler.Imports.Helper
{
    public class ImportHelper(DataContext context) : IImportHelper
    {
        private const int Timeout = 300;
        private readonly DataContext _context = context;
        private static readonly ConcurrentDictionary<ImportDataType, StagingTableMapping> _mappingCache = new();

        public async Task<ImportResult> ProcessImportAsync(ImportDataType dataType, Guid companyId, DataTable dataTable, CancellationToken ct)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return new ImportResult { AffectedIds = [], RowsProcessed = 0, HasAffectedIds = false };
            }

            var mapping = await GetMappingAsync(dataType, ct);
            var connection = (SqlConnection)_context.Database.GetDbConnection();
            await using var outerTransaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // Step 0: Get the sql transaction
                SqlTransaction outerSqlTransaction = (SqlTransaction)outerTransaction.GetDbTransaction();

                // Step 1: Refresh staging table (versioning)
                await RefreshStagingTableAsync(connection, outerSqlTransaction, dataType, companyId, ct);

                // Step 2: Bulk insert into staging table
                await BulkInsertAsync(connection, outerSqlTransaction, dataTable, mapping.TableName, ct);
                await outerTransaction.CommitAsync(ct);
                await using var innerTransacton = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    // Step 3: Execute stored procedure
                    SqlTransaction innerSqlTransaction = (SqlTransaction)innerTransacton.GetDbTransaction();
                    var result = await ExecuteStoredProcedureAsync(connection, innerSqlTransaction, mapping.ProcedureName, companyId, ct);
                    result.RowsProcessed = dataTable.Rows.Count;

                    await innerTransacton.CommitAsync(ct);
                    return result;
                }
                catch (Exception ex)
                {
                    await innerTransacton.RollbackAsync(ct);
                    throw new Exception($"Inner transaction failed to import {dataType} for company {companyId}", ex);
                }
            }
            catch (Exception ex)
            {
                await outerTransaction.RollbackAsync(ct);
                throw new Exception($"Outer transaction failed to import {dataType} for company {companyId}", ex);
            }
        }

        private async Task<StagingTableMapping> GetMappingAsync(ImportDataType dataType, CancellationToken ct)
        {
            if (_mappingCache.TryGetValue(dataType, out var cachedMapping))
                return cachedMapping;

            var mapping = await _context.Database
                .SqlQueryRaw<StagingTableMapping>(
                    @"SELECT TableName, ProcedureName 
                      FROM dbo.StagingTableMapping 
                      WHERE ImportDataType = @importDataType",
                    new SqlParameter("@importDataType", dataType)
                )
                .SingleOrDefaultAsync(ct);

            if (mapping == null)
                throw new InvalidOperationException($"No mapping found for ImportDataType: {dataType}");

            var result = new StagingTableMapping(mapping.TableName, mapping.ProcedureName);
            _mappingCache.TryAdd(dataType, result);
            return result;
        }

        private static async Task RefreshStagingTableAsync(SqlConnection connection, SqlTransaction transaction, ImportDataType dataType, Guid companyId, CancellationToken ct)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.RefreshStagingTable";
            command.CommandTimeout = Timeout;

            command.Parameters.AddWithValue("@CompanyId", companyId);
            command.Parameters.AddWithValue("@ImportDataType", dataType);

            await command.ExecuteNonQueryAsync(ct);
        }

        private static async Task BulkInsertAsync(SqlConnection connection, SqlTransaction transaction, DataTable dataTable, string tableName, CancellationToken ct)
        {
            using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = $"dbo.{tableName}",
                BatchSize = 50000,
                BulkCopyTimeout = Timeout
            };

            await bulk.WriteToServerAsync(dataTable, ct);
        }

        private static async Task<ImportResult> ExecuteStoredProcedureAsync(SqlConnection connection, SqlTransaction transaction, string procedureName, Guid companyId, CancellationToken ct)
        {
            var result = new ImportResult();

            using var command = new SqlCommand($"dbo.{procedureName}", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = Timeout
            };

            command.Parameters.AddWithValue("@CompanyId", companyId);

            using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                try
                {
                    // Try adding Ids of affected rows
                    result.AffectedIds.Add(reader.GetGuid(0));
                    result.HasAffectedIds = true;
                }
                catch (InvalidCastException)
                {
                    // The result set doesn't contain GUIDs - that's fine
                    result.HasAffectedIds = false;
                    break;
                }
            }

            return result;
        }

        private record StagingTableMapping(string TableName, string ProcedureName);
    }
}
