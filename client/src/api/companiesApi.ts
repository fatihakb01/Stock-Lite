import type { Company } from "../features/companies/companyTypes";
import api from "./axios";

export const getCompany = async (): Promise<Company> => {
  const res = await api.get(`/company`);
  return res.data;
};
