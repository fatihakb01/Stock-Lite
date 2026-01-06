import { useQuery } from "@tanstack/react-query";
import type { PurchaseOrder } from "../types/purchaseOrders/purchaseOrderTypes";
import { getPurchaseOrders } from "../api/purchaseOrdersApi";

export const usePurchaseOrders = (productId: string | null) => {
  return useQuery<PurchaseOrder[]>({
    queryKey: ["purchaseorder", productId],
    queryFn: () => getPurchaseOrders(productId!),
    enabled: !!productId
  });
};