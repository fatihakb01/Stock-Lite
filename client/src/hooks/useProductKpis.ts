import { useQuery } from "@tanstack/react-query"
import { getProductKpi, getSchedulerStatus } from "../api/productKpisApi"
import type { ProductKpi, SchedulerStatus } from "../types/productKpis/productKpiTypes"
import { useCurrentUser } from "./useUsers"

export const useProductKpi = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<ProductKpi>({
        queryKey: ["productkpis", currentUser?.companyId],
        queryFn: () => getProductKpi()
    })
}

export const useSchedulerStatus = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<SchedulerStatus>({
        queryKey: ["schedulerstatus", currentUser?.companyId],
        queryFn: () => getSchedulerStatus()
    })
}