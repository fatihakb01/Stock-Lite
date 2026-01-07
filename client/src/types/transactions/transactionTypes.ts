export type Transaction = {
    transactionId: string,
    totalAmount: number,
    transactionDate: Date,
    paymentMethod?: string,
    notes?: string,
    lineItems: [{
        productId: string,
        quantity: number,
        unitPrice: number
    }]
}
