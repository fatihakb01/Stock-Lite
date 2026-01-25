export const importDataTypeMap: Record<number, string> = {
    0: "Products",
    1: "Transactions",
    2: "ConfirmedOrders",
    3: "PurchaseOrders"
};

export const reverseImportDataTypeMap: Record<string, number> = {
    Products: 0,
    Transactions: 1,
    ConfirmedOrders: 2,
    PurchaseOrders: 3
};
