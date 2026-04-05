import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CreateExportProfile, DeleteExportProfile, EditExportProfile, GetExportProfile, GetExportProfiles, RunExportProfile } from "../api/exportProfileApi";
import type { CreateExportProfileDto, EditExportProfileDto, ExportProfile, ExportProfileDetail } from "../types/exportProfiles/exportProfileTypes";

export const useExportProfiles = () => {
    return useQuery<ExportProfile[]>({
        queryKey: ["exportprofiles"],
        queryFn: () => GetExportProfiles()
    })
}

export const useExportProfile = (id: string | null) => {
    return useQuery<ExportProfileDetail>({
        queryKey: ["exportprofile", id],
        queryFn: () => GetExportProfile(id!),
        enabled: !!id
    })
}

export const useRunExportProfile = (id: string | null) => {
    return useMutation<void>({
        mutationFn: () => RunExportProfile(id!),
    })
}

export const useCreateExportProfile = () => {
    const queryClient = useQueryClient();

    return useMutation<ExportProfileDetail, Error, CreateExportProfileDto>({
        mutationFn: (dto) => CreateExportProfile(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["exportprofile"] });
            queryClient.invalidateQueries({ queryKey: ["exportprofiles"] });
        }
    })
}

export const useEditExportProfile = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<ExportProfileDetail, Error, EditExportProfileDto>({
        mutationFn: (dto) => EditExportProfile(id!, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["exportprofile"] });
            queryClient.invalidateQueries({ queryKey: ["exportprofiles"] });
        }
    })
}

export const useDeleteExportProfile = (id: string | null) => {
    const queryClient = useQueryClient();

    return useMutation<void>({
        mutationFn: () => DeleteExportProfile(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["exportprofile"] });
            queryClient.invalidateQueries({ queryKey: ["exportprofiles"] });
        }
    })
}