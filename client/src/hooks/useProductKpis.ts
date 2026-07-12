import { useQuery } from "@tanstack/react-query"
import { getProductKpi, getSchedulerStatus } from "../api/productKpisApi"
import type { ProductKpi, SchedulerStatus } from "../types/productKpis/productKpiTypes"

export const useProductKpi = () => {
    return useQuery<ProductKpi>({
        queryKey: ["productkpi", "home"],
        queryFn: () => getProductKpi()
    })
}

export const useSchedulerStatus = () => {
    return useQuery<SchedulerStatus>({
        queryKey: ["productkpi", "schedulerStatus"],
        queryFn: () => getSchedulerStatus()
    })
}