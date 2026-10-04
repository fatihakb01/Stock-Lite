import { 
    useMutation, 
    useQuery
} from "@tanstack/react-query"
import type { 
    CreateScheduledJobDto, 
    EditScheduledJobDto, 
    ReorderScheduledJobsDto, 
    ScheduledJob,
    ScheduledJobExecution
} from "../types/scheduledJobs/scheduledJobTypes"
import { 
    CleanScheduledJob,
    CreateScheduledJob, 
    DeleteScheduledJob, 
    EditScheduledJob, 
    GetScheduledJob, 
    GetScheduledJobHistory, 
    ReorderScheduledJobs, 
    RunScheduledJob 
} from "../api/scheduledJobsApi"

export const useScheduledJob = (id: string | null) => {
    return useQuery<ScheduledJob>({
        queryKey: ["scheduledjob", id],
        queryFn: () => GetScheduledJob(id!),
        enabled: !!id
    })
}

export const useScheduledJobHistory = (id: string | null, enabled: boolean) => {
    return useQuery<ScheduledJobExecution[]>({
        queryKey: ["scheduledjob", "history", id],
        queryFn: () => GetScheduledJobHistory(id!),
        enabled: !!id && enabled
    })
}

export const useRunScheduledJob = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => RunScheduledJob(id!)
    })
}

export const useCleanScheduledJob = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => CleanScheduledJob(id!)
    })
}

export const useReorderScheduledJobs = () => {
    return useMutation<void, Error, ReorderScheduledJobsDto>({
        mutationFn: (dto) => ReorderScheduledJobs(dto),
    })
}

export const useCreateScheduledJob = () => {
    return useMutation<ScheduledJob, Error, CreateScheduledJobDto>({
        mutationFn: (dto) => CreateScheduledJob(dto),
    })
}

export const useEditScheduledJob = (id: string | null) => {
    return useMutation<ScheduledJob, Error, EditScheduledJobDto>({
        mutationFn: (dto) => EditScheduledJob(id!, dto)
    })
}

export const useDeleteScheduledJob = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => DeleteScheduledJob(id!)
    })
}
