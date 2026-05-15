using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class TransactionHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal async Task<Transaction> CreateTransaction(
            Company company,
            DateTime transactionDate,
            string transactionId = "TR-000001"
        )
        {
            var transaction = new Transaction
            {
                TransactionId = transactionId,
                TransactionDate = transactionDate,
                Company = company,
                CompanyId = company.Id
            };

            await _fixture.Context.Transactions.AddAsync(transaction);
            await _fixture.Context.SaveChangesAsync();

            return transaction;
        }

        internal async Task<TransactionLineItem> CreateTransactionLineItem(
            Transaction transaction,
            Product product,
            int quantity,
            decimal unitPrice
        )
        {
            var lineItem = new TransactionLineItem
            {
                TransactionId = transaction.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Transaction = transaction,
                Product = product
            };

            await _fixture.Context.TransactionLineItems.AddAsync(lineItem);
            await _fixture.Context.SaveChangesAsync();

            return lineItem;
        }

        private static string BuildCsvHeaders()
        {
            return "TransactionId;SKU;TransactionDate;PaymentMethod;Notes;Quantity\r\n";
        }

        internal static string BuildCsv(
            string transactionId = "T-0001",
            string sku = "SKU-0001",
            string transactionDate = "2026-05-01",
            string paymentMethod = "",
            string notes = "",
            int quantity = 10,
            bool includeHeaders = true
            )
        {
            string content = $"{transactionId};{sku};{transactionDate};{paymentMethod};{notes};{quantity}";
            if (includeHeaders)
            {
                return BuildCsvHeaders() + $"{content}\r\n";
            }

            return $"{content}\r\n";
        }

        internal async Task<int> CountTransactions(Guid companyId)
        {
            return await _fixture.Context.TransactionLineItems
                .CountAsync(x => x.Product.CompanyId == companyId);
        }
    }
}
