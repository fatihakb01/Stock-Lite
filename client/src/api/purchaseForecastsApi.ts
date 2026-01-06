import type { PurchaseForecast } from "../types/purchaseForecasts/purchaseForecastTypes";
import api from "./axios";

export const getPurchaseForecasts = async (productId: string) : Promise<PurchaseForecast[]> => {
  const res = await api.get(`/purchaseforecast/${productId}`);
  return res.data;
};