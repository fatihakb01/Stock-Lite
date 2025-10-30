using System.Globalization;
using Application.Core;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.SalesAggregates.Features.Commands;

public class UpdateSalesAggregatesOld
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PeriodType { get; set; } = "Daily";
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<TransactionLineItem> transactionLineItemRepo,
        ISalesAggregateRepository aggregateRepo)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<Unit>.Failure($"Invalid period type: {request.PeriodType}", 400);

            var transactions = await transactionLineItemRepo.FindAsync(
                x => x.ProductId == request.ProductId,
                cancellationToken,
                x => x.Transaction,
                x => x.Product
            );

            transactions = transactions
                .Where(x => x.Transaction != null)
                .ToList();

            if (!transactions.Any())
                return Result<Unit>.Failure("No transactions found for this product", 404);

            IEnumerable<SalesAggregate> aggregates = parsedPeriod switch
            {
                Period.Daily => AggregateDaily(transactions, request.ProductId),
                Period.Weekly => AggregateWeekly(transactions, request.ProductId),
                Period.Monthly => AggregateMonthly(transactions, request.ProductId),
                _ => AggregateDaily(transactions, request.ProductId)
            };

            await aggregateRepo.UpsertSalesAggregatesAsync(aggregates);
            return Result<Unit>.Success(Unit.Value);
        }
    }

    private static IEnumerable<SalesAggregate> AggregateDaily
        (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
    {
        IEnumerable<IGrouping<object, TransactionLineItem>> grouped = transactions
            .GroupBy(t => (object)t.Transaction!.TransactionDate.Date);

        var aggregates = new List<SalesAggregate>();

        foreach (var g in grouped)
        {
            DateTime start, end;
            start = g.First().Transaction!.TransactionDate.Date;
            end = start;

            aggregates.Add(new SalesAggregate
            {
                ProductId = ProductId,
                PeriodType = Period.Daily,
                PeriodStartDate = start,
                PeriodEndDate = end,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice),
                Product = g.First().Product
            });
        }

        return aggregates;
    }

    private static IEnumerable<SalesAggregate> AggregateWeekly
        (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
    {
        IEnumerable<IGrouping<object, TransactionLineItem>> grouped = transactions
            .GroupBy(t =>
            {
                var d = t.Transaction!.TransactionDate;
                return (object)new { Year = ISOWeek.GetYear(d), Week = ISOWeek.GetWeekOfYear(d) };
            });

        var aggregates = new List<SalesAggregate>();

        foreach (var g in grouped)
        {
            DateTime start, end;
            var first = g.First().Transaction!.TransactionDate;
            var year = ISOWeek.GetYear(first);
            var week = ISOWeek.GetWeekOfYear(first);
            start = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
            end = start.AddDays(6);

            aggregates.Add(new SalesAggregate
            {
                ProductId = ProductId,
                PeriodType = Period.Weekly,
                PeriodStartDate = start,
                PeriodEndDate = end,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice),
                Product = g.First().Product
            });
        }

        return aggregates;
    }

    private static IEnumerable<SalesAggregate> AggregateMonthly
        (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
    {
        IEnumerable<IGrouping<object, TransactionLineItem>> grouped = transactions
            .GroupBy(t =>
            {
                var d = t.Transaction!.TransactionDate;
                return (object)new { d.Year, d.Month };
            });

        var aggregates = new List<SalesAggregate>();

        foreach (var g in grouped)
        {
            DateTime start, end;
            var first = g.First().Transaction!.TransactionDate;
            start = new DateTime(first.Year, first.Month, 1);
            end = start.AddMonths(1).AddDays(-1);

            aggregates.Add(new SalesAggregate
            {
                ProductId = ProductId,
                PeriodType = Period.Weekly,
                PeriodStartDate = start,
                PeriodEndDate = end,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice),
                Product = g.First().Product
            });
        }

        return aggregates;
    }
}
