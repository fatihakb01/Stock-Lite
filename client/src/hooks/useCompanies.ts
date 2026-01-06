import { useQuery } from "@tanstack/react-query";
import type { Company } from "../types/companies/companyTypes";
import { getCompany } from "../api/companiesApi";

export const useCompanies = () => {
  return useQuery<Company>({
    queryKey: ["company"],
    queryFn: () => getCompany()
  });
};