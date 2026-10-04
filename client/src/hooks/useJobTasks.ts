import { 
    useMutation, 
    useQuery, 
    useQueryClient 
} from "@tanstack/react-query"
import type { 
    JobTaskDetail, 
    JobTask, 
    CreateJobTaskDto, 
    EditJobTaskDto, 
    JobTaskExecution
} from "../types/jobTasks/jobTaskTypes"
import { 
    CleanJobTask,
    CreateJobTask, 
    DeleteJobTask, 
    EditJobTask, 
    GetJobTask, 
    GetJobTaskHistory, 
    GetJobTasks, 
    GetRecentHistory, 
    RunJobTask 
} from "../api/jobTasksApi"
import { useCurrentUser } from "./useUsers";

export const useJobTaskHistory = (id: string | null, enabled: boolean) => {
    return useQuery<JobTaskExecution[]>({
        queryKey: [`jobtask:history:${id}`],
        queryFn: () => GetJobTaskHistory(id!),
        enabled: !!id && enabled
    });
}

export const useJobTasks = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<JobTask[]>({
        queryKey: ["jobtasks", currentUser?.companyId],
        queryFn: () => GetJobTasks(),
        enabled: !!currentUser?.companyId
    });
}

export const useJobTask = (id: string | null) => {
    return useQuery<JobTaskDetail>({
        queryKey: ["jobtask", id],
        queryFn: () => GetJobTask(id!),
        enabled: !!id
    });
}

export const useRecentHistory = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<JobTaskExecution[]>({
        queryKey: [`jobtasks:recent:history:${currentUser?.companyId}`],
        queryFn: () => GetRecentHistory(),
        enabled: !!currentUser?.companyId
    })
}

export const useRunJobTask = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => RunJobTask(id!)
    })
}

export const useCleanJobTask = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => CleanJobTask(id!)
    })
}

export const useCreateJobTask = () => {
    return useMutation<JobTask, Error, CreateJobTaskDto>({
        mutationFn: (dto) => CreateJobTask(dto)
    })
}

export const useEditJobTask = (id: string | null) => {
    return useMutation<JobTask, Error, EditJobTaskDto>({
        mutationFn: (dto) => EditJobTask(id!, dto)
    })
}

export const useDeleteJobTask = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => DeleteJobTask(id!)
    })
}
