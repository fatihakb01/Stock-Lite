export type Transaction = {
    transactionId: string,
    totalAmount: number,
    transactionDate: Date,
    paymentMethod?: string,
    notes?: string,
    customerId: string,
    lineItems: {
        productId: string,
        quantity: number,
        unitPrice: number
    }
}
