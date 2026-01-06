import { usePurchaseForecasts } from "../../../../hooks/usePurchaseForecasts";

interface ProductPfcTabProps {
  productId: string | null;
}

export default function ProductPfcTab({
    productId
} : ProductPfcTabProps) {

    const { 
    data: purchaseForecasts, 
    isLoading: isLoading, 
    isError: isError
    } = usePurchaseForecasts(productId);

    console.log(purchaseForecasts);

    return (
        <>
            Test
        </>
    )
};