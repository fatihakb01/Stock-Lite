import type { ConfirmedOrder } from "../types/confirmedOrders/confirmedOrderTypes";
import api from "./axios";

export const getConfirmedOrders = async (productId: string): Promise<ConfirmedOrder[]> => {
  const res = await api.get(`/confirmedorder/by-product/${productId}`);
  return res.data;
};