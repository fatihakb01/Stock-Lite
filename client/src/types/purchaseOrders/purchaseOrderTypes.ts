export type PurchaseOrder = {
    orderNumber: string,
    status: string,
    orderDate: string,
    expectedDeliveryDate?: string,
    receivedDate?: string,
    companyId: string
    linItems: {
        productId: string,
        quantity: number,
        receivedQuantity: number,
        unitCost: number
    }
}