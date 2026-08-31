using Domain.Enums;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories.Helper
{
    internal class CallProcedureHelper(DataContext context)
    {
        private readonly DataContext _context = context;
        internal async Task ExecuteProcAsync(
            string procName,
            Guid companyId,
            IEnumerable<Guid>? productIds,
            Period? period = null
        )
        {
            var paramCompany = new SqlParameter("@CompanyId", companyId);

            var productIdsTable = new DataTable();
            productIdsTable.Columns.Add("Id", typeof(Guid));
            if (productIds != null)
                foreach (var id in productIds)
                    productIdsTable.Rows.Add(id);

            var paramProductIds = new SqlParameter("@ProductIds", productIdsTable)
            {
                TypeName = "dbo.GuidList",
                SqlDbType = SqlDbType.Structured
            };

            FormattableString sql;

            if (period is null)
            {
                sql = $"EXEC {procName} {paramCompany}, {paramProductIds}";
            }
            else
            {
                var paramPeriod = new SqlParameter(
                    "@PeriodType",
                    period.ToString());

                sql = $"EXEC {procName} {paramCompany}, {paramProductIds}, {paramPeriod}";
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(CancellationToken.None);

            try
            {
                await _context.Database.ExecuteSqlAsync(sql);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Transaction failed to execute procedure {procName} for company {companyId}", ex);
            }
        }
    }
}
