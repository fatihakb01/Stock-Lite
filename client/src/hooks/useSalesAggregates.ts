import { useQuery } from "@tanstack/react-query";
import type { SalesAggregate } from "../features/salesaggregates/salesaggregateTypes";
import { getSalesAggregates } from "../api/salesaggregatesApi";

export const useSalesAggregates = (periodType: string | null, productId: string | null) => {
  return useQuery<SalesAggregate[]>({
    queryKey: ["salesaggregate", periodType, productId],
    queryFn: () => getSalesAggregates(periodType!, productId!),
    enabled: !!(periodType && productId)
  });
};
