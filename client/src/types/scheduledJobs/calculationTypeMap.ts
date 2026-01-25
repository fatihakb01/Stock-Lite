export const calculationTypeMap: Record<number, string> = {
    0: "StockLevels",
    1: "ConfirmedOrderAggregates",
    2: "Forecasts",
    3: "SalesAggregates",
    4: "PurchaseForecasts"
};

export const reverseCalculationTypeMap: Record<string, number> = {
    StockLevels: 0,
    ConfirmedOrderAggregates: 1,
    Forecasts: 2,
    SalesAggregates: 3,
    PurchaseForecasts: 4
};
