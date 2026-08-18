import { useQuery } from "@tanstack/react-query"
import { getProductKpi, getSchedulerStatus } from "../api/productKpisApi"
import type { ProductKpi, SchedulerStatus } from "../types/productKpis/productKpiTypes"
import { useCurrentUser } from "./useUsers"

export const useProductKpi = () => {
    return useQuery<ProductKpi>({
        queryKey: ["productkpi", "home"],
        queryFn: () => getProductKpi()
    })
}

export const useSchedulerStatus = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<SchedulerStatus>({
        queryKey: ["productkpi", "schedulerStatus", currentUser?.companyId],
        queryFn: () => getSchedulerStatus()
    })
}