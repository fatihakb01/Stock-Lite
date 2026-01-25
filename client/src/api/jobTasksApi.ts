import type 
{ 
    CreateJobTaskDto, 
    EditJobTaskDto, 
    JobExecution, 
    JobTask, 
    JobTaskAndExecutions, 
    JobTaskDetail
} from "../types/jobTasks/jobTaskTypes"
import api from "./axios"

export const GetJobTaskHistory = async (id: string) : Promise<JobTaskAndExecutions> => {
    const res = await api.get(`/jobtask/history/${id}`);
    return res.data;
}

export const GetJobTasks = async () : Promise<JobTask[]> => {
    const res = await api.get('/jobtask');
    return res.data;
}

export const GetJobTask =  async (id: string) : Promise<JobTaskDetail> => {
    const res = await api.get(`/jobtask/${id}`);
    return res.data;
}

export const GetRecentHistory = async () : Promise<JobExecution[]> => {
    const res = await api.get('/jobtask/history');
    return res.data;
}

export const RunJobTask = async (id: string) : Promise<void> => {
    await api.post(`jobtask/${id}`);
}

export const CreateJobTask = async (dto: CreateJobTaskDto) : Promise<JobTask> => {
    const res = await api.post('/jobtask', dto);
    return res.data;
}

export const EditJobTask = async (id: string, dto: EditJobTaskDto) : Promise<JobTask> => {
    const res = await api.put(`/jobtask/${id}`, dto);
    return res.data;
}

export const DeleteJobTask = async (id: string) : Promise<void> => {
    await api.delete(`/jobtask/${id}`);
}
