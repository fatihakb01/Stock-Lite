import type { ExportProfile, ExportProfileDetail, CreateExportProfileDto, EditExportProfileDto } from "../types/exportProfiles/exportProfileTypes";
import api from "./axios";

export const GetExportProfiles = async (): Promise<ExportProfile[]> => {
    const res = await api.get('/exportprofile');
    return res.data;
}

export const GetExportProfile = async (id: string): Promise<ExportProfileDetail> => {
    const res = await api.get(`/exportprofile/${id}`);
    return res.data;
}

export const RunExportProfile = async (id: string): Promise<void> => {
    const res = await api.post(`/exportprofile/run/${id}`);
    return res.data;
}

export const CreateExportProfile = async (dto: CreateExportProfileDto): Promise<ExportProfileDetail> => {
    const res = await api.post('/exportprofile', dto);
    return res.data;
}

export const EditExportProfile = async (id: string, dto: EditExportProfileDto): Promise<ExportProfileDetail> => {
    const res = await api.put(`/exportprofile/${id}`, dto);
    return res.data;
}

export const DeleteExportProfile = async (id: string): Promise<void> => {
    const res = await api.delete(`/exportprofile/${id}`);
    return res.data;
}