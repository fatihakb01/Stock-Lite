import { useQuery } from "@tanstack/react-query";
import type { Forecast } from "../types/forecasts/forecastTypes";
import { getForecasts } from "../api/forecastsApi";

export const useForecasts = (periodType: string | null, productId: string | null) => {
    return useQuery<Forecast[]>({
        queryKey: ["forecast", periodType, productId],
        queryFn: () => getForecasts(periodType!, productId!),
        enabled: !!(periodType && productId)
    });
};
