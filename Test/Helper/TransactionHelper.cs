using Domain.Entities.Core;

namespace Test.Helper
{
    internal static class TransactionHelper
    {
        internal static Transaction CreateTransaction(
            Company company,
            DateTime transactionDate,
            string transactionId = "TR-000001"
        ) => new Transaction
        {
            TransactionId = transactionId,
            TransactionDate = transactionDate,
            Company = company,
            CompanyId = company.Id
        };

        internal static List<Transaction> CreateTransactions(
            Company company,
            int total = 5,
            int lookBackDays = 1095
        )
        {
            List<Transaction> transactions = [];
            Random random = new Random();
            DateTime now = DateTime.Now;

            for (int i = 1; i <= total; i++)
            {
                string transactionId = "TR-000000" + i;
                int randomNumber = random.Next(lookBackDays);
                DateTime transactionDate = now.AddDays(-randomNumber);

                Transaction transaction = CreateTransaction(
                    company: company,
                    transactionId: transactionId, 
                    transactionDate: transactionDate
                );

                transactions.Add(transaction);
            }

            return transactions;
        }

        internal static TransactionLineItem CreateTransactionLineItem(
            Transaction transaction,
            Product product,
            int quantity,
            decimal unitPrice
        ) => new TransactionLineItem
        {
            TransactionId = transaction.Id,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Transaction = transaction,
            Product = product
        };

        internal static List<TransactionLineItem> CreateTransactionLineItems(
            Transaction transaction,
            Product product,
            int maxRandomQuantity = 10,
            double maxRandomUnitPrice = 99.99,
            int total = 5
        )
        {
            List<TransactionLineItem> lineItems = [];
            Random random = new Random();

            for (int i = 1; i <= total; i++)
            {
                int randomQuantity = random.Next(maxRandomQuantity);
                decimal randomUnitPrice = (decimal) (random.NextDouble() * maxRandomUnitPrice);

                TransactionLineItem lineItem = CreateTransactionLineItem(
                    transaction: transaction,
                    product: product,
                    quantity: randomQuantity,
                    unitPrice: randomUnitPrice
                );

                lineItems.Add(lineItem);
            }

            return lineItems;
        }

        internal static (Transaction transaction, List<TransactionLineItem> lineItems) CreateTransactionAndLineItems(
            Company company,
            Product product,
            string transactionId = "TR-0000001",
            int maxLineItems = 5,
            int lookBackDays = 1095,
            int maxRandomQuantity = 10,
            double maxRandomUnitPrice = 99.99
        )
        {
            Random random = new Random();
            DateTime now = DateTime.Now;

            int randomNumber = random.Next(lookBackDays);
            DateTime transactionDate = now.AddDays(-randomNumber);

            Transaction transaction = CreateTransaction(
                company: company,
                transactionDate: transactionDate,
                transactionId: transactionId
            );

            List<TransactionLineItem> lineItems = CreateTransactionLineItems(
                transaction: transaction,
                product: product,
                maxRandomQuantity: maxRandomQuantity,
                maxRandomUnitPrice: maxRandomUnitPrice,
                total: maxLineItems
            );

            return (transaction, lineItems);
        }
    }
}
