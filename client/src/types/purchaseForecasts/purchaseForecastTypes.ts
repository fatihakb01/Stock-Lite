export type PurchaseForecast = {
    productId: string,
    weekStartDate: Date,
    weekEndDate: string,
    weekIndex: number,
    forecastQuantity: number,
    quantityCommitted: number,
    quantityOnOrder: number,
    projectedStock: number,
    needsReorder: boolean,
    recommendedOrderQuantity: number,
    recommendedOrderDate?: string,
    calculatedAt: string
}