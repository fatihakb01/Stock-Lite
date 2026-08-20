import { useQuery } from "@tanstack/react-query";
import type { PurchaseOrder } from "../types/purchaseOrders/purchaseOrderTypes";
import { getPurchaseOrders } from "../api/purchaseOrdersApi";
import { useCurrentUser } from "./useUsers";

export const usePurchaseOrders = (productId: string | null) => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<PurchaseOrder[]>({
        queryKey: ["purchaseorder", productId],
        queryFn: () => getPurchaseOrders(productId!),
        enabled: !!productId && !!currentUser?.companyId
    });
};