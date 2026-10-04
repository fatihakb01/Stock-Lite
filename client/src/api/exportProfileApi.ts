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

export const StreamExportProfile = async (id: string, fileType: string): Promise<void> => {
    const res = await api.post(`/exportprofile/stream/${id}?fileType=${fileType}`, null, {
        responseType: "blob"
    });

    const blob = new Blob([res.data], { type: res.headers["content-type"]?.toString() });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");

    const disposition = res.headers["content-disposition"];
    const fileName = disposition
        ? disposition.split("filename=")[1]?.replace(/"/g, "")
        : `export.${fileType === "Csv" ? "csv" : "xlsx"}`;

    a.href = url;
    a.download = fileName;
    a.click();

    URL.revokeObjectURL(url);
};

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