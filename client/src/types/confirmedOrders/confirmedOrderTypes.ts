export type ConfirmedOrder = {
    orderNumber: string,
    status: string,
    orderDate: Date,
    expectedDeliveryDate: Date,
    receivedDate?: Date,
    companyId: string
    lineItems: [{
        productId: string,
        quantity: number,
        soldQuantity: number,
        unitCost: number
    }]
}