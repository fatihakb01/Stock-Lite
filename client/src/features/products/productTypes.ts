export type Product = {
    id: string,
    name: string,
    description? : string,
    sku?: string,
    price: number,
    createdAt: Date
    isActive: boolean
    stock: {
        quantity: number,
        reorderPoint: number
    }
}