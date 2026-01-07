import type { PurchaseOrder } from "../types/purchaseOrders/purchaseOrderTypes";
import api from "./axios";

export const getPurchaseOrders = async (productId: string): Promise<PurchaseOrder[]> => {
  const res = await api.get(`/purchaseorder/by-product/${productId}`);
  return res.data;
};