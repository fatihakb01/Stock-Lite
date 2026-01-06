import { useTransactions } from "../../../../hooks/useTransactions";

interface ProductTransactionsTabProps {
  productId: string | null;
}

export default function ProductTransactionsTab({
    productId
} : ProductTransactionsTabProps) {

    const { 
    data: transactions, 
    isLoading: isLoading, 
    isError: isError
    } = useTransactions(productId);

    console.log(transactions);

    return (
        <>
            Test
        </>
    )
};