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

        connection.on("ProductConfirmedOrdersUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["confirmedorder", updatedProductId] });
            }
        });

        connection.on("ProductUserDefinedFieldsUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["userdefinedfield", updatedProductId] });
            }
        });

        connection.on("ProductForecastUpdated", (updatedProductId: string, period: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["forecast", period, updatedProductId] });
            }
        });

        connection.on("ProductSalesAggregateUpdated", (updatedProductId: string, period: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["salesaggregate", period, updatedProductId] });
            }
        });

        connection.on("ProductConfirmedOrderAggregateUpdated", (updatedProductId: string, period: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["confirmedorderaggregate", period, updatedProductId] });
            }
        });

        connection.on("ProductPurchaseForecastUpdated", (updatedProductId: string) => {
            if (productId === updatedProductId) {
                queryClient.invalidateQueries({ queryKey: ["purchaseforecast", updatedProductId] });
            }
        });

        startProductConnection(connection, productId);

        return () => {
            connection.off("ProductUpdated");
            connection.off("ProductTransactionsUpdated");
            connection.off("ProductPurchaseOrdersUpdated");
            connection.off("ProductConfirmedOrdersUpdated");
            connection.off("ProductUserDefinedFieldsUpdated");
            connection.off("ProductForecastUpdated");
            connection.off("ProductSalesAggregateUpdated");
            connection.off("ProductConfirmedOrderAggregateUpdated");
            connection.off("ProductPurchaseForecastUpdated");
            leaveProductGroup(connection, productId);
        };
    }, [queryClient, productId]);
}