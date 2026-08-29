import { useQuery } from "@tanstack/react-query"
import { getProductKpi, getSchedulerStatus } from "../api/productKpisApi"
import type { ProductKpi, SchedulerStatus } from "../types/productKpis/productKpiTypes"
import { useCurrentUser } from "./useUsers"

const { data: currentUser } = useCurrentUser();

export const useProductKpi = () => {
    return useQuery<ProductKpi>({
        queryKey: ["productkpi", "home", currentUser?.companyId],
        queryFn: () => getProductKpi()
    })
}

export const useSchedulerStatus = () => {
    return useQuery<SchedulerStatus>({
        queryKey: ["productkpi", "schedulerStatus", currentUser?.companyId],
        queryFn: () => getSchedulerStatus()
    })
}