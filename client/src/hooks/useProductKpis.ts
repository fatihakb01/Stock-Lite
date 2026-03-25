import { useQuery } from "@tanstack/react-query"
import { getProductKpi, getSchedulerStatus } from "../api/productKpisApi"
import type { ProductKpi } from "../types/productKpis/productKpiTypes"

export const useProductKpi = () => {
    return useQuery<ProductKpi>({
        queryKey: ["productkpi-home"],
        queryFn: () => getProductKpi()
    })
}

export const useSchedulerStatus = () => {
    return useQuery<string>({
        queryKey: ["productkpi-schedulerStatus"],
        queryFn: () => getSchedulerStatus()
    })
}