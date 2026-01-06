import { useQuery } from "@tanstack/react-query";
import type { Transaction } from "../types/transactions/transactionTypes";
import { getTransactions } from "../api/transactionsApi";

export const useTransactions = (productId: string | null) => {
  return useQuery<Transaction[]>({
    queryKey: ["transaction", productId],
    queryFn: () => getTransactions(productId!),
    enabled: !!productId
  });
};