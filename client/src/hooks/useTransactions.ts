import { useQuery } from "@tanstack/react-query";
import type { Transaction } from "../types/transactions/transactionTypes";
import { getTransactions } from "../api/transactionsApi";
import { useCurrentUser } from "./useUsers";

export const useTransactions = (productId: string | null) => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<Transaction[]>({
        queryKey: ["transaction", productId],
        queryFn: () => getTransactions(productId!),
        enabled: !!productId && !!currentUser?.companyId
    });
};