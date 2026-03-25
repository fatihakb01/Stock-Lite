export type ProductKpi = {
    schedulerStatus: string,
    totalProductCount: number,
    belowSafetyStockCount: number,
    recommendedOrdersCount: number,
    withoutStockAtLeadTimeCount: number,
    withoutForecastCount: number
}