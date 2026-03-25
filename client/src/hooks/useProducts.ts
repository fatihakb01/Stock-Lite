import { useQuery } from "@tanstack/react-query";
import { getProducts, getProduct } from "../api/productsApi";
import type { Product, ProductDetail } from "../types/products/productTypes";

export const useProducts = (filter?: string) => {
  return useQuery<Product[]>({
    queryKey: ["products", filter],
    queryFn: () => getProducts(filter!),
    enabled: filter !== null
  });
};

export const useProduct = (id: string | null) => {
  return useQuery<ProductDetail>({
    queryKey: ["product", id],
    queryFn: () => getProduct(id!),
    enabled: !!id
  });
};