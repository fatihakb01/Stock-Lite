import type {   
    CreateScheduledJobDto, 
    EditScheduledJobDto, 
    ScheduledJob,
    ScheduledJobExecution
} from "../types/scheduledJobs/scheduledJobTypes";
import api from "./axios";

export const GetScheduledJob = async (id: string) : Promise<ScheduledJob> => {
    const res = await api.get(`/scheduledjob/${id}`);
    return res.data;
}

export const GetScheduledJobHistory = async (id: string) : Promise<ScheduledJobExecution[]> => {
    const res = await api.get(`/scheduledjob/history/${id}`);
    return res.data;
}

export const RunScheduledJob = async (id: string) : Promise<void> => {
    const res = await api.post(`/scheduledjob/${id}`);
    return res.data;
}

export const CreateScheduledJob = async (dto: CreateScheduledJobDto) : Promise<ScheduledJob> => {
    const res = await api.post('/scheduledjob', dto);
    return res.data;
}

export const EditScheduledJob = async (id: string, dto: EditScheduledJobDto) : Promise<ScheduledJob> => {
    const res = await api.put(`/scheduledjob/${id}`, dto);
    return res.data;
}

export const DeleteScheduledJob = async (id: string) : Promise<void> => {
    const res = await api.delete(`/scheduledjob/${id}`);
    return res.data;
}
