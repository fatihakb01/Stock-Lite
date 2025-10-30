using System.Globalization;
using Application.Core;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.SalesAggregates.Features.Commands;

public class UpdateSalesAggregates
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
        public Guid CompanyId { get; set; }
    }

    public class Handler(
        IRepository<TransactionLineItem> transactionLineItemRepo,
        IRepository<Product> productRepo,
        ISalesAggregateRepository aggregateRepo)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Get products to process
            IEnumerable<Guid> productIds;

            if (request.ProductIds != null && request.ProductIds.Any())
            {
                productIds = request.ProductIds.ToList();
            }
            else
            {
                productIds = await productRepo.FindSelectAsync(
                    p => p.CompanyId == request.CompanyId,
                    p => p.Id,
                    cancellationToken
                );
            }

            if (!productIds.Any())
                return Result<Unit>.Failure("No products found to aggregate", 404);

            // Load all transactions for selected products
            var transactions = await transactionLineItemRepo.FindAsync(
                x => productIds.Contains(x.ProductId),
                cancellationToken,
                x => x.Transaction,
                x => x.Product
            );

            transactions = transactions.Where(x => x.Transaction != null).ToList();

            if (!transactions.Any())
                return Result<Unit>.Failure("No transactions found for selected products", 404);

            // Aggregate per product
            var allAggregates = new List<SalesAggregate>();

            foreach (var productId in productIds)
            {
                var productTransactions = transactions.Where(t => t.ProductId == productId);
                if (!productTransactions.Any()) continue;

                foreach (var period in new[] { Period.Daily, Period.Weekly, Period.Monthly })
                {
                    var aggregates = period switch
                    {
                        Period.Daily => AggregateDaily(productTransactions, productId),
                        Period.Weekly => AggregateWeekly(productTransactions, productId),
                        Period.Monthly => AggregateMonthly(productTransactions, productId),
                        _ => AggregateDaily(productTransactions, productId)
                    };

                    allAggregates.AddRange(aggregates);
                }
            }

            // Persist all updated aggregates in one go
            await aggregateRepo.UpsertSalesAggregatesAsync(allAggregates);

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
                PeriodType = Period.Monthly,
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
