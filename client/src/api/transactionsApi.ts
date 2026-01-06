import type { Transaction } from "../types/transactions/transactionTypes";
import api from "./axios";

export const getTransactions = async (productId: string): Promise<Transaction[]> => {
  const res = await api.get(`/transaction/${productId}`);
  return res.data;
};