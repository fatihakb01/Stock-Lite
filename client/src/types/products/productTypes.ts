export type Product = {
    id: string,
    name: string,
    description? : string,
    sku?: string,
    price: number,
    createdAt: Date
    isActive: boolean
}

export type ProductDetail = {
    id: string,
    name: string,
    description? : string,
    sku?: string,
    price: number,
    createdAt: Date
    isActive: boolean
    stockLevel: {
        quantityOnHand: number,
        quantityOnOrder: number,
        quantityCommitted: number,
        netStockPosition: number,
        forecastQuantity: number,
        reorderPoint: number,
        needsReorder: boolean,
        recommendedOrderQuantity: number
    },
    inventoryPolicy: {
        leadTimeDays: number,
        minimumOrderQuantity: number,
        orderIncrementQuantity: number,
        safetyStock: number,
        stockAdjustment: number
    }
}

export type ProductKpi = {
    schedulerFailed: boolean,
    totalProductCount: number,
    belowSafetyStockCount: number,
    recommendedOrdersCount: number,
    withoutStockAtLeadTimeCount: number,
    withoutForecastCount: number
}
