using Domain.Entities.Core;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class PurchaseOrderHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        private readonly static DateTime Now = DateTime.UtcNow.Date;
        internal async Task<PurchaseOrder> CreatePurchaseOrder(
            Company company,
            string orderNumber = "PO-0001",
            DateTime? expectedDeliveryDate = null,
            DateTime? receivedDate = null,
            PurchaseOrderStatus status = PurchaseOrderStatus.Completed
        )
        {
            var po = new PurchaseOrder
            {
                CompanyId = company.Id,
                Company = company,
                OrderNumber = orderNumber,
                OrderDate = Now,
                ExpectedDeliveryDate = expectedDeliveryDate ?? Now.AddDays(7),
                ReceivedDate = receivedDate,
                Status = status
            };

            await _fixture.Context.PurchaseOrder.AddAsync(po);
            await _fixture.Context.SaveChangesAsync();

            return po;
        }

        internal async Task<PurchaseOrderLineItem> CreatePurchaseOrderLineItem(
            PurchaseOrder order,
            Product product,
            int quantity = 20,
            int receivedQuantity = 0,
            decimal unitCost = 10
        )
        {
            var poli = new PurchaseOrderLineItem
            {
                PurchaseOrderId = order.Id,
                PurchaseOrder = order,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                ReceivedQuantity = receivedQuantity,
                UnitCost = unitCost
            };

            await _fixture.Context.PurchaseOrderLineItem.AddAsync(poli);
            await _fixture.Context.SaveChangesAsync();

            return poli;
        }

        private static string BuildCsvHeaders()
        {
            return
                "OrderNumber;SKU;OrderDate;ExpectedDeliveryDate;ReceivedDate;Quantity;ReceivedQuantity\r\n";
        }

        internal static string BuildCsv(
            string orderNumber = "PO-0001",
            string sku = "SKU-0001",
            int quantity = 10,
            int receivedQuantity = 10,
            string orderDate = "2026-05-01",
            string expectedDeliveryDate = "2026-05-02",
            string? receivedDate = "2026-05-02",
            bool includeHeaders = true
            )
        {
            string content = $"{orderNumber};{sku};{orderDate};{expectedDeliveryDate};{receivedDate};{quantity};{receivedQuantity}";
            if (includeHeaders)
            {
                return BuildCsvHeaders() + $"{content}\r\n";
            }

            return $"{content}\r\n";
        }

        internal async Task<int> CountPurchaseOrders(Guid companyId)
        {
            return await _fixture.Context.PurchaseOrderLineItem
                .CountAsync(x => x.Product.CompanyId == companyId);
        }

        internal async Task<PurchaseOrder?> GetPurchaseOrder(
            Guid companyId,
            string orderNumber)
        {
            return await _fixture.Context.PurchaseOrder
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.OrderNumber == orderNumber);
        }

        internal async Task<PurchaseOrderLineItem?> GetLineItem(
            Guid companyId,
            string orderNumber,
            string sku)
        {
            return await _fixture.Context.PurchaseOrderLineItem
                .Include(x => x.Product)
                .Include(x => x.PurchaseOrder)
                .FirstOrDefaultAsync(x =>
                    x.Product.SKU == sku &&
                    x.PurchaseOrder.OrderNumber == orderNumber &&
                    x.PurchaseOrder.CompanyId == companyId);
        }
    }
}