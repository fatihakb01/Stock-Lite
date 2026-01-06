interface ProductTransactionsTabProps {
  productId: string | null;
}

export default function ProductTransactionsTab({
    productId
} : ProductTransactionsTabProps) {

    console.log(productId);

    return (
        <>
            Test
        </>
    )
};