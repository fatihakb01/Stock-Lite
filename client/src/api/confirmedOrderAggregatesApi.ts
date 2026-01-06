import type { ConfirmedOrderAggregate } from "../types/confirmedOrderAggregates/confirmedOrderAggregateTypes";
import api from "./axios";

export const getConfirmedOrderAggregates = async (periodType: string, productId: string): 
Promise<ConfirmedOrderAggregate[]> => {
  const res = await api.get(`/confirmedorderaggregate/${periodType}/${productId}`);
  return res.data;
};
