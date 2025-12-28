import { useQuery } from "@tanstack/react-query";
import { getConfirmedOrderAggregates } from "../api/confirmedOrderAggregatesApi";
import type { ConfirmedOrderAggregate } from "../features/confirmedOrderAggregates/confirmedOrderAggregatesTypes";

export const useConfirmedOrderAggregates = (periodType: string | null, productId: string | null) => {
  return useQuery<ConfirmedOrderAggregate[]>({
    queryKey: ["confirmedorderaggregate", periodType, productId],
    queryFn: () => getConfirmedOrderAggregates(periodType!, productId!),
    enabled: !!(periodType && productId)
  });
};