export type PurchaseOrder = {
    orderNumber: string,
    status: string,
    orderDate: string,
    expectedDeliveryDate: string,
    receivedDate?: string,
    companyId: string
    lineItems: [{
        productId: string,
        quantity: number,
        receivedQuantity: number,
        unitCost: number
    }]
}