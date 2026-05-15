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

        internal async Task<int> CountTransactions(Guid companyId)
        {
            return await _fixture.Context.TransactionLineItems
                .CountAsync(x => x.Product.CompanyId == companyId);
        }
    }
}
