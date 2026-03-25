import { 
    useMutation, 
    useQuery, 
    useQueryClient 
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
        queryKey: ["scheduledjob-history", id],
        queryFn: () => GetScheduledJobHistory(id!),
        enabled: !!id && enabled
    })
}

export const useRunScheduledJob = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => RunScheduledJob(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useCleanScheduledJob = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => CleanScheduledJob(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["jobtasks"] });
            queryClient.invalidateQueries({ queryKey: ["jobtask"] });
            queryClient.invalidateQueries({ queryKey: ["scheduledjob"] });
        }
    })
}

export const useReorderScheduledJobs = () => {
    const queryClient = useQueryClient();

    return useMutation<void, Error, ReorderScheduledJobsDto>({
        mutationFn: (dto) => ReorderScheduledJobs(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useCreateScheduledJob = () => {
    const queryClient = useQueryClient();

    return useMutation<ScheduledJob, Error, CreateScheduledJobDto>({
        mutationFn: (dto) => CreateScheduledJob(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useEditScheduledJob = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<ScheduledJob, Error, EditScheduledJobDto>({
        mutationFn: (dto) => EditScheduledJob(id!, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useDeleteScheduledJob = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => DeleteScheduledJob(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}
