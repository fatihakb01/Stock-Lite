import type { SalesAggregate } from "../features/salesaggregates/salesaggregateTypes";
import api from "./axios";

export const getSalesAggregates = async (periodType: string, productId: string): 
Promise<SalesAggregate[]> => {
  const res = await api.get(`/salesaggregate/${periodType}/${productId}`);
  return res.data;
};
