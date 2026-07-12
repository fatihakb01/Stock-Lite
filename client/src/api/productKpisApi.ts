import type { ProductKpi, SchedulerStatus } from "../types/productKpis/productKpiTypes";
import api from "./axios";

export const getProductKpi = async (): Promise<ProductKpi> => {
    const res = await api.get('/productkpi/home');
    return res.data;
}

export const getSchedulerStatus = async (): Promise<SchedulerStatus> => {
    const res = await api.get('/productkpi/schedulerStatus');
    return res.data;
}