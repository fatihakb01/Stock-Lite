import api from "./axios";
import type { Product, ProductDetail } from "../features/products/productTypes";

export const getProducts = async (): Promise<Product[]> => {
  const res = await api.get('/product');
  return res.data;
};

export const getProduct = async (id: string): Promise<ProductDetail> => {
  const res = await api.get(`/product/${id}`);
  return res.data;
};
