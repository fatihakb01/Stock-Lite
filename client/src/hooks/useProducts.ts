import { useQuery } from "@tanstack/react-query";
import { getProducts, getProduct, getProductKpi } from "../api/productsApi";
import type { Product, ProductDetail, ProductKpi } from "../types/products/productTypes";

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

export const useProductKpi = () => {
  return useQuery<ProductKpi>({
    queryKey: ["product-kpis"],
    queryFn: () => getProductKpi()
  })
}