import { useQuery } from "@tanstack/react-query";
import type { PurchaseForecast } from "../types/purchaseForecasts/purchaseForecastTypes";
import { getPurchaseForecasts } from "../api/purchaseForecastsApi";

export const usePurchaseForecasts = (productId: string | null) => {
  return useQuery<PurchaseForecast[]>({
    queryKey: ["purchaseforecast", productId],
    queryFn: () => getPurchaseForecasts(productId!),
    enabled: !!productId
  });
};