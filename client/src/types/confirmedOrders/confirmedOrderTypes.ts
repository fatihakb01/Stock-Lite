export type ConfirmedOrder = {
    orderNumber: string,
    status: string,
    orderDate: string,
    expectedDeliveryDate: string,
    receivedDate?: string,
    companyId: string
    linItems: {
        productId: string,
        quantity: number,
        soldQuantity: number,
        unitCost: number
    }
}