using Domain.Entities.Core;
using Domain.Enums;
using Domain.Services.Scheduler;
using Domain.Services.Scheduler.Calculations;
using Infrastructure.Context;
using Infrastructure.Services.Imports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class JobDispatcher(IServiceProvider provider, DataContext context) : IJobDispatcher
{
    private readonly IServiceProvider _provider = provider;
    private readonly DataContext _context = context;

    public async Task DispatchAsync(
        ScheduledJob job,
        CancellationToken ct
    )
    {
        switch (job.JobType)
        {
            case JobType.Import:
                await HandleImport(job, ct);
                break;

            case JobType.StandardCalculation:
                await HandleStandardCalculation(job, ct);
                break;

            case JobType.CustomCalculation:
                await HandleCustomCalculation(job, ct);
                break;

            default:
                throw new InvalidOperationException("Unsupported job type");
        }
    }

    private async Task HandleImport(ScheduledJob job, CancellationToken ct)
    {
        if (job.ImportDataType == null)
            throw new InvalidOperationException("Import job requires ImportDataType.");

        if (string.IsNullOrWhiteSpace(job.Source))
            throw new InvalidOperationException("Import job requires a Source.");

        using var scope = _provider.CreateScope();

        var coordinator = scope.ServiceProvider
            .GetRequiredService<ImportCoordinator>();

        await coordinator.RunAsync(
            job.ImportDataType.Value,
            job.Source,
            job.JobTask.CompanyId,
            ct
        );
    }

    private async Task HandleStandardCalculation(
        ScheduledJob job,
        CancellationToken ct)
    {
        using var scope = _provider.CreateScope();

        var companyId = job.JobTask.CompanyId;
        var period = job.PeriodType;

        switch (job.CalculationType)
        {
            case StandardCalculationType.StockLevels:
                var stockLevels =
                    scope.ServiceProvider.GetRequiredService<IStockLevelCalculation>();

                await stockLevels.RunAsync(companyId, null, ct);
                break;

            case StandardCalculationType.ConfirmedOrderAggregates:
                var confirmedOrders =
                    scope.ServiceProvider.GetRequiredService<IConfirmedOrderAggregateCalculation>();

                await confirmedOrders.RunAsync(companyId, null, period, ct);
                break;

            case StandardCalculationType.Forecasts:
                var forecasts =
                    scope.ServiceProvider.GetRequiredService<IForecastCalculation>();

                await forecasts.RunAsync(companyId, null, period, ct);
                break;

            case StandardCalculationType.SalesAggregates:
                var salesaggregates =
                    scope.ServiceProvider.GetRequiredService<ISalesAggregateCalculation>();

                await salesaggregates.RunAsync(companyId, null, period, ct);
                break;

            case StandardCalculationType.PurchaseForecasts:
                var confirmedorderaggregates =
                    scope.ServiceProvider.GetRequiredService<IConfirmedOrderAggregateCalculation>();

                await confirmedorderaggregates.RunAsync(companyId, null, period, ct);
                break;
            default:
                throw new InvalidOperationException(
                    $"CalculationType is required for job {job.Id}"
                );
        }
    }

    private async Task HandleCustomCalculation(
        ScheduledJob job,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(job.Target))
            throw new InvalidOperationException("Custom calculation requires a target procedure.");

        // Enforce naming convention
        if (!job.Target.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
            && !job.Target.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid custom procedure name.");
        }

        // Normalize name
        var procName = job.Target.Contains('.')
            ? job.Target
            : $"dbo.{job.Target}";

        var exists = await _context.Database
            .SqlQueryRaw<int>(
                @"
                SELECT COUNT(*) AS Value
                FROM sys.procedures p
                JOIN sys.schemas s ON p.schema_id = s.schema_id
                WHERE s.name + '.' + p.name = @procName
                ",
                new SqlParameter("@procName", procName)
            )
            .SingleAsync(ct);

        if (exists == 0)
            throw new InvalidOperationException($"Stored procedure '{procName}' does not exist.");

        // Execute procedure
        await _context.Database.ExecuteSqlAsync($"EXEC {procName}", ct);
    }
}
