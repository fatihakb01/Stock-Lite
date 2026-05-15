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
            ConfirmedOrderStatus status = ConfirmedOrderStatus.Confirmed
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
            await _fixture.Context.SaveChangesAsync();

            return co;
        }

        internal async Task<ConfirmedOrderLineItem> CreateConfirmedOrderLineItem(
            ConfirmedOrder order,
            Product product,
            int quantity = 10,
            int soldQuantity = 0,
            decimal unitCost = 10
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
            await _fixture.Context.SaveChangesAsync();

            return coli;
        }

        internal async Task<int> CountConfirmedOrders(Guid companyId)
        {
            return await _fixture.Context.ConfirmedOrderLineItem
                .CountAsync(x => x.Product.CompanyId == companyId);
        }
    }
}