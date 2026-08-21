import { useQuery } from "@tanstack/react-query";
import { getUserDefinedFields } from "../api/userDefinedFieldsApi";
import type { UserDefinedField } from "../types/userDefinedFields/userDefinedFieldTypes";
import { useCurrentUser } from "./useUsers";

export const useUserDefinedFields = (productId: string | null) => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<UserDefinedField[]>({
        queryKey: ["userdefinedfield", productId],
        queryFn: () => getUserDefinedFields(productId!),
        enabled: !!productId && !!currentUser?.companyId
    });
};