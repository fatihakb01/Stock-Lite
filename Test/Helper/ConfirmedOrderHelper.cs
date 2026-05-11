using Domain.Entities.Core;
using Domain.Enums;

namespace Test.Helper
{
    internal static class ConfirmedOrderHelper
    {
        private readonly static DateTime Now = DateTime.UtcNow.Date;
        internal static ConfirmedOrder CreateConfirmedOrder(
            Company company,
            string orderNumber = "CO-0001",
            DateTime? expectedDeliveryDate = null,
            DateTime? receivedDate = null,
            ConfirmedOrderStatus status = ConfirmedOrderStatus.Confirmed
        ) => new ConfirmedOrder
        {
            CompanyId = company.Id,
            Company = company,
            OrderNumber = orderNumber,
            OrderDate = Now,
            ExpectedDeliveryDate = expectedDeliveryDate ?? Now.AddDays(7),
            ReceivedDate = receivedDate,
            Status = status
        };

        internal static ConfirmedOrderLineItem CreateConfirmedOrderLineItem(
            ConfirmedOrder order,
            Product product,
            int quantity = 10,
            int soldQuantity = 0,
            decimal unitCost = 10
        ) => new ConfirmedOrderLineItem
        {
            ConfirmedOrderId = order.Id,
            ConfirmedOrder = order,
            ProductId = product.Id,
            Product = product,
            Quantity = quantity,
            SoldQuantity = soldQuantity,
            UnitCost = unitCost
        };
    }
}