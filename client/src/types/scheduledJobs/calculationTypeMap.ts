export const calculationTypeMap: Record<number, string> = {
    0: "StockLevels",
    1: "ConfirmedOrderAggregates",
    2: "Forecasts",
    3: "SalesAggregates",
    4: "PurchaseForecasts",
    5: "ProductKpis",
    6: "Seasonality"
};

export const reverseCalculationTypeMap: Record<string, number> = {
    StockLevels: 0,
    ConfirmedOrderAggregates: 1,
    Forecasts: 2,
    SalesAggregates: 3,
    PurchaseForecasts: 4,
    ProductKpis: 5,
    Seasonality: 6
};
