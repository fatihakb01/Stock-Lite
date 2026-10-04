import { useQuery } from "@tanstack/react-query";
import type { ConfirmedOrder } from "../types/confirmedOrders/confirmedOrderTypes";
import { getConfirmedOrders } from "../api/confirmedOrdersApi";
import { useCurrentUser } from "./useUsers";

export const useConfirmedOrders = (productId: string | null) => {
    return useQuery<ConfirmedOrder[]>({
        queryKey: ["confirmedorder", productId],
        queryFn: () => getConfirmedOrders(productId!),
        enabled: !!productId
    });
};