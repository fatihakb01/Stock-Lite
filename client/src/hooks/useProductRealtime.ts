import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getProductConnection, leaveProductGroup, startProductConnection } from "../signalR/productConnection";
import { useCurrentUser } from "./useUsers";

export function useProductRealtime(productId?: string | null) {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    useEffect(() => {
        if (!currentUser) return;

        const connection = getProductConnection();

        connection.on("ProductUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["product", updatedProductId] });
            }
        });

        connection.on("ProductTransactionsUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["transaction", updatedProductId] });
            }
        });

        connection.on("ProductPurchaseOrdersUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["purchaseorder", updatedProductId] });
            }
        });

        startProductConnection(connection, productId);

        return () => {
            connection.off("ProductUpdated");
            connection.off("ProductTransactionsUpdated");
            connection.off("ProductPurchaseOrdersUpdated");
            leaveProductGroup(connection, productId);
        };
    }, [queryClient, productId]);
}