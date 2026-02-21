import type { UserDefinedField } from "../types/userDefinedFields/userDefinedFieldTypes";
import api from "./axios";

export const getUserDefinedFields = async (productId: string): Promise<UserDefinedField[]> => {
  const res = await api.get(`/userdefinedfield/${productId}`);
  return res.data;
};