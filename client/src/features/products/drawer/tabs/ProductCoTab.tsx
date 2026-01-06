import { useConfirmedOrders } from "../../../../hooks/useConfirmedOrders";

interface ProductCoTabProps {
  productId: string | null;
}

export default function ProductCoTab({
    productId
} : ProductCoTabProps) {

    const { 
    data: confirmedOrders, 
    isLoading: isLoading, 
    isError: isError
    } = useConfirmedOrders(productId);

    console.log(confirmedOrders);

    return (
        <>
            Test
        </>
    )
};