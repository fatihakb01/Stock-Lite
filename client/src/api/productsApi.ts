import api from "./axios";
import type { Product } from "../features/products/productTypes";

export const getProducts = async (): Promise<Product[]> => {
  const res = await api.get('/product');
  return res.data;
};

export const getProduct = async (id: string): Promise<Product> => {
  const res = await api.get(`/product/${id}`);
  return res.data;
};
