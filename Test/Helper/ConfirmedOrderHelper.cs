using Domain.Entities.Core;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class ConfirmedOrderHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        private readonly static DateTime Now = DateTime.UtcNow.Date;
        internal async Task<ConfirmedOrder> CreateConfirmedOrder(
            Company company,
            string orderNumber = "CO-0001",
            DateTime? expectedDeliveryDate = null,
            DateTime? receivedDate = null,
            ConfirmedOrderStatus status = ConfirmedOrderStatus.Confirmed,
            bool saveChanges = true
        )
        {
            var co = new ConfirmedOrder
            {
                CompanyId = company.Id,
                Company = company,
                OrderNumber = orderNumber,
                OrderDate = Now,
                ExpectedDeliveryDate = expectedDeliveryDate ?? Now.AddDays(7),
                ReceivedDate = receivedDate,
                Status = status
            };

            await _fixture.Context.ConfirmedOrder.AddAsync(co);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return co;
        }

        internal async Task<ConfirmedOrderLineItem> CreateConfirmedOrderLineItem(
            ConfirmedOrder order,
            Product product,
            int quantity = 10,
            int soldQuantity = 0,
            decimal unitCost = 10,
            bool saveChanges = true
        )
        {
            var coli = new ConfirmedOrderLineItem
            {
                ConfirmedOrderId = order.Id,
                ConfirmedOrder = order,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                SoldQuantity = soldQuantity,
                UnitCost = unitCost
            };

            await _fixture.Context.ConfirmedOrderLineItem.AddAsync(coli);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return coli;
        }

        private static string BuildCsvHeaders()
        {
            return
                "OrderNumber;SKU;OrderDate;ExpectedDeliveryDate;ReceivedDate;Quantity;SoldQuantity\r\n";
        }

        internal static string BuildCsv(
            string orderNumber = "CO-0001",
            string sku = "SKU-0001",
            int quantity = 10,
            int soldQuantity = 10,
            string orderDate = "2026-05-01",
            string expectedDeliveryDate = "2026-05-02",
            string? receivedDate = "2026-05-02",
            bool includeHeaders = true
            )
        {
            string content = $"{orderNumber};{sku};{orderDate};{expectedDeliveryDate};{receivedDate};{quantity};{soldQuantity}";
            if (includeHeaders)
            {
                return BuildCsvHeaders() +$"{content}\r\n";
            }

            return $"{content}\r\n";
        }

        internal async Task<int> CountConfirmedOrders(Guid companyId)
        {
            return await _fixture.Context.ConfirmedOrderLineItem
                .CountAsync(x => x.Product.CompanyId == companyId);
        }

        internal async Task<ConfirmedOrder?> GetConfirmedOrder(
            Guid companyId,
            string orderNumber)
        {
            return await _fixture.Context.ConfirmedOrder
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.OrderNumber == orderNumber);
        }

        internal async Task<ConfirmedOrderLineItem?> GetLineItem(
            Guid companyId,
            string orderNumber,
            string sku)
        {
            return await _fixture.Context.ConfirmedOrderLineItem
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.ConfirmedOrder)
                .FirstOrDefaultAsync(x =>
                    x.Product.SKU == sku &&
                    x.ConfirmedOrder.OrderNumber == orderNumber &&
                    x.ConfirmedOrder.CompanyId == companyId);
        }
    }
}