import { useQuery } from "@tanstack/react-query";
import { getProducts, getProduct } from "../api/productsApi";
import type { Product, ProductDetail } from "../types/products/productTypes";
import { useCurrentUser } from "./useUsers";

export const useProducts = (filter?: string) => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<Product[]>({
        queryKey: ["products", currentUser?.companyId, filter],
        queryFn: () => getProducts(filter!),
        enabled: filter !== null && !!currentUser?.companyId
    });
};

export const useProduct = (id: string | null) => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<ProductDetail>({
        queryKey: ["product", id],
        queryFn: () => getProduct(id!),
        enabled: !!id && !!currentUser?.companyId
    });
};