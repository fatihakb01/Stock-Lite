import type { Forecast } from "../types/forecasts/forecastTypes";
import api from "./axios";

export const getForecasts = async (periodType: string, productId: string): 
Promise<Forecast[]> => {
  const res = await api.get(`/forecast/${periodType}/${productId}`);
  return res.data;
};
