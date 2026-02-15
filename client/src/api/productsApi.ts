import api from "./axios";
import type { Product, ProductDetail, ProductKpi } from "../types/products/productTypes";

export const getProducts = async (filter?: string): Promise<Product[]> => {
  const url = 
    filter === undefined ? 
    "/product" : 
    `/product?filter=${filter}`;

  const res = await api.get(url);

  return res.data;
};

export const getProduct = async (id: string): Promise<ProductDetail> => {
  const res = await api.get(`/product/${id}`);
  return res.data;
};

export const getProductKpi = async (): Promise<ProductKpi> => {
  const res = await api.get('/product/kpis');
  return res.data;
}
