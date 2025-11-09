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
        ISalesAggregateRepository aggregateRepo,
        IDateTimeProvider dateTimeProvider)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Initialize table for sales aggregates
            await aggregateRepo.EnsureAllMissingPeriodsAsync(request.CompanyId);

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

            int productNr = 0;
            int totalProducts = productIds.Count();

            foreach (var productId in productIds)
            {
                var productTransactions = transactions.Where(t => t.ProductId == productId);
                if (!productTransactions.Any()) continue;

                // Aggregate based on transactions
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

                Console.WriteLine($"Product {++productNr}/{totalProducts} completed.");
            }

            // Persist all updated aggregates in one go
            await aggregateRepo.UpsertSalesAggregatesAsync(allAggregates);

            return Result<Unit>.Success(Unit.Value);
        }

        private IEnumerable<SalesAggregate> AggregateDaily
            (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
        {
            var grouped = transactions
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

        private IEnumerable<SalesAggregate> AggregateWeekly
            (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
        {
            var grouped = transactions
                .GroupBy(t =>
                {
                    var d = t.Transaction!.TransactionDate;
                    return (object)new { Year = dateTimeProvider.GetIsoYear(d), Week = dateTimeProvider.GetIsoWeekOfYear(d) };
                });

            var aggregates = new List<SalesAggregate>();

            foreach (var g in grouped)
            {
                var first = g.First().Transaction!.TransactionDate;
                var (start, end) = dateTimeProvider.GetWeekRange(first);

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

        private IEnumerable<SalesAggregate> AggregateMonthly
            (IEnumerable<TransactionLineItem> transactions, Guid ProductId)
        {
            var grouped = transactions
                .GroupBy(t =>
                {
                    var d = t.Transaction!.TransactionDate;
                    return (object)new { d.Year, d.Month };
                });

            var aggregates = new List<SalesAggregate>();

            foreach (var g in grouped)
            {
                var first = g.First().Transaction!.TransactionDate;
                var (start, end) = dateTimeProvider.GetMonthRange(first);

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
}
