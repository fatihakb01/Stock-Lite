import type { Company } from "../types/companies/companyTypes";
import api from "./axios";

export const getCompany = async (): Promise<Company> => {
  const res = await api.get(`/company`);
  return res.data;
};
