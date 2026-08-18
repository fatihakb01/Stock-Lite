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
    const { data: currentUser } = useCurrentUser();

    return useQuery<JobTaskExecution[]>({
        queryKey: ["jobtask", "history", id],
        queryFn: () => GetJobTaskHistory(id!),
        enabled: !!id && enabled && !!currentUser?.companyId
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
    const { data: currentUser } = useCurrentUser();

    return useQuery<JobTaskDetail>({
        queryKey: ["jobtask", id],
        queryFn: () => GetJobTask(id!),
        enabled: !!id && !!currentUser?.companyId
    });
}

export const useRecentHistory = () => {
    const { data: currentUser } = useCurrentUser();

    return useQuery<JobTaskExecution[]>({
        queryKey: ["jobtask", "recent-history", currentUser?.companyId],
        queryFn: () => GetRecentHistory(),
        enabled: !!currentUser?.companyId
    })
}

export const useRunJobTask = (id: string | null) => {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    return useMutation<void>({
        mutationFn: () => RunJobTask(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["jobtask", id] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser?.companyId] });
            // queryClient.invalidateQueries({ queryKey: ["scheduledjob"] });
            queryClient.invalidateQueries({ queryKey: ["productkpi", "schedulerStatus", currentUser?.companyId] });
        }
    })
}

export const useCleanJobTask = (id: string | null) => {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    return useMutation<void>({
        mutationFn: () => CleanJobTask(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["jobtask", id] });
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser?.companyId] });
            // queryClient.invalidateQueries({ queryKey: ["scheduledjob"] });
        }
    })
}

export const useCreateJobTask = () => {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    return useMutation<JobTask, Error, CreateJobTaskDto>({
        mutationFn: (dto) => CreateJobTask(dto),
        onSuccess: () => {
            // queryClient.invalidateQueries({queryKey: ["jobtask"]});
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser?.companyId]});
            // queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useEditJobTask = (id: string | null) => {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    return useMutation<JobTask, Error, EditJobTaskDto>({
        mutationFn: (dto) => EditJobTask(id!, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask", id]});
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser?.companyId]});
            // queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}

export const useDeleteJobTask = (id: string | null) => {
    const queryClient = useQueryClient();
    const { data: currentUser } = useCurrentUser();

    return useMutation<void>({
        mutationFn: () => DeleteJobTask(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["jobtask", id]});
            queryClient.invalidateQueries({ queryKey: ["jobtasks", currentUser?.companyId]});
            // queryClient.invalidateQueries({queryKey: ["scheduledjob"]});
        }
    })
}
