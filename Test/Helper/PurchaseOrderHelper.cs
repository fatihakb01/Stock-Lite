using Domain.Entities.Core;
using Domain.Enums;

namespace Test.Helper
{
    internal static class PurchaseOrderHelper
    {
        private readonly static DateTime Now = DateTime.UtcNow.Date;
        internal static PurchaseOrder CreatePurchaseOrder(
            Company company,
            string orderNumber = "PO-0001",
            DateTime? expectedDeliveryDate = null,
            DateTime? receivedDate = null,
            PurchaseOrderStatus status = PurchaseOrderStatus.Completed
        ) => new PurchaseOrder
        {
            CompanyId = company.Id,
            Company = company,
            OrderNumber = orderNumber,
            OrderDate = Now,
            ExpectedDeliveryDate = expectedDeliveryDate ?? Now.AddDays(7),
            ReceivedDate = receivedDate,
            Status = status
        };

        internal static PurchaseOrderLineItem CreatePurchaseOrderLineItem(
            PurchaseOrder order,
            Product product,
            int quantity = 20,
            int receivedQuantity = 0,
            decimal unitCost = 10
        ) => new PurchaseOrderLineItem
        {
            PurchaseOrderId = order.Id,
            PurchaseOrder = order,
            ProductId = product.Id,
            Product = product,
            Quantity = quantity,
            ReceivedQuantity = receivedQuantity,
            UnitCost = unitCost
        };
    }
}