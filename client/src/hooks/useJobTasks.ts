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
    CreateJobTask, 
    DeleteJobTask, 
    EditJobTask, 
    GetJobTask, 
    GetJobTaskHistory, 
    GetJobTasks, 
    GetRecentHistory, 
    RunJobTask 
} from "../api/jobTasksApi"

export const useJobTaskHistory = (id: string | null) => {
    return useQuery<JobTaskExecution[]>({
        queryKey: ["jobtask-history", id],
        queryFn: () => GetJobTaskHistory(id!),
        enabled: !!id
    });
}

export const useJobTasks = () => {
    return useQuery<JobTask[]>({
        queryKey: ["jobtasks"],
        queryFn: () => GetJobTasks()
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
    return useQuery<JobTaskExecution[]>({
        queryKey: ["jobtask-recent-history"],
        queryFn: () => GetRecentHistory()
    })
}

export const useRunJobTask = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => RunJobTask(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
            queryClient.invalidateQueries({queryKey: ["jobtask-history"]});
            queryClient.invalidateQueries({queryKey: ["jobtask-recent-history"]});
            queryClient.invalidateQueries({queryKey: ["confirmedorderaggregate"]});
            queryClient.invalidateQueries({queryKey: ["confirmedorder"]});
            queryClient.invalidateQueries({queryKey: ["forecast"]});
            queryClient.invalidateQueries({queryKey: ["product"]});
            queryClient.invalidateQueries({queryKey: ["purchaseforecast"]});
            queryClient.invalidateQueries({queryKey: ["salesaggregate"]});
        }
    })
}

export const useCreateJobTask = () => {
    const queryClient = useQueryClient();

    return useMutation<JobTask, Error, CreateJobTaskDto>({
        mutationFn: (dto) => CreateJobTask(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useEditJobTask = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<JobTask, Error, EditJobTaskDto>({
        mutationFn: (dto) => EditJobTask(id!, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useDeleteJobTask = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => DeleteJobTask(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({queryKey: ["jobtasks"]});
            queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}
