using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class ProductNotificationService(INotificationService notificationService) : IProductNotificationService
    {
        public async Task NotifyUpdated(object prefix, List<Guid> productIds, object? o, CancellationToken ct)
        {
            Period? period = o != null ? (Period)o : null;
            switch (prefix)
            {
                case ProductKey.Product:
                    await notificationService.NotifyProductGroups(productIds, "ProductUpdated", null, ct);
                    break;
                case TransactionKey.Transaction:
                    await notificationService.NotifyProductGroups(productIds, "ProductTransactionsUpdated", null, ct);
                    break;
                case PurchaseOrderKey.PurchaseOrder:
                    await notificationService.NotifyProductGroups(productIds, "ProductPurchaseOrdersUpdated", null, ct);
                    break;
                case ConfirmedOrderKey.ConfirmedOrder:
                    await notificationService.NotifyProductGroups(productIds, "ProductConfirmedOrdersUpdated", null, ct);
                    break;
                case UserDefinedFieldKey.UserDefinedField:
                    await notificationService.NotifyProductGroups(productIds, "ProductUserDefinedFieldsUpdated", null, ct);
                    break;
                case ForecastKey.Forecast:
                    await notificationService.NotifyProductGroups(productIds, "ProductForecastUpdated", period, ct);
                    break;
                case SalesAggregateKey.SalesAggregate:
                    await notificationService.NotifyProductGroups(productIds, "ProductSalesAggregateUpdated", period, ct);
                    break;
                case ConfirmedOrderAggregateKey.ConfirmedOrderAggregate:
                    await notificationService.NotifyProductGroups(productIds, "ProductConfirmedOrderAggregateUpdated", period, ct);
                    break;
                case PurchaseForecastKey.PurchaseForecast:
                    await notificationService.NotifyProductGroups(productIds, "ProductPurchaseForecastUpdated", null, ct);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported key type: {prefix}");
            }
        }
    }
}
