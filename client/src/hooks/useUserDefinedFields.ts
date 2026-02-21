import { useQuery } from "@tanstack/react-query";
import { getUserDefinedFields } from "../api/userDefinedFieldsApi";
import type { UserDefinedField } from "../types/userDefinedFields/userDefinedFieldTypes";

export const useUserDefinedFields = (productId: string | null) => {
  return useQuery<UserDefinedField[]>({
    queryKey: ["userdefinedfield", productId],
    queryFn: () => getUserDefinedFields(productId!),
    enabled: !!productId
  });
};