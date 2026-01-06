import { usePurchaseOrders } from "../../../../hooks/usePurchaseOrders";

interface ProductPoTabProps {
  productId: string | null;
}

export default function ProductPoTab({
    productId
} : ProductPoTabProps) {

    const { 
      data: purchaseOrders, 
      isLoading: isLoading, 
      isError: isError 
    } = usePurchaseOrders(productId);

    console.log(purchaseOrders);

    return (
        <>
            Test
        </>
    )
};