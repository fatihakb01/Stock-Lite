import { useQuery, /*useMutation, useQueryClient*/ } from "@tanstack/react-query";
import { getProducts/*, createProduct*/, getProduct } from "../api/productsApi";
import type { Product } from "../features/products/productTypes";

export const useProducts = (companyId: string | null) => {
  return useQuery<Product[]>({
    queryKey: ["product", companyId],
    queryFn: () => getProducts(companyId!),
    enabled: !!companyId
  });
};

export const useProduct = (id: string | null, companyId: string | null) => {
  return useQuery<Product>({
    queryKey: ["product", id, companyId],
    queryFn: () => getProduct(id!, companyId!),
    enabled: !!(id && companyId)
  });
};

// export const useCreateProduct = () => {
//   const queryClient = useQueryClient();

//   return useMutation({
//     mutationFn: createProduct,
//     onSuccess: () => {
//       queryClient.invalidateQueries({ queryKey: ["product"] });
//     },
//   });
// };
