import { useQuery } from "@tanstack/react-query";
import { getProducts, getProduct } from "../api/productsApi";
import type { Product } from "../features/products/productTypes";

export const useProducts = () => {
  return useQuery<Product[]>({
    queryKey: ["product"],
    queryFn: () => getProducts()
  });
};

export const useProduct = (id: string | null) => {
  return useQuery<Product>({
    queryKey: ["product", id],
    queryFn: () => getProduct(id!),
    enabled: !!id
  });
};
