export type ExportProfile = {
    id: string,
    name: string,
    exportType?: string,
    customType?: string,
    targetPath?: string
}

export type ExportProfileDetail = {
    id: string,
    name: string,
    description?: string,
    exportType?: string,
    customType?: string,
    customSource?: string,
    targetPath?: string
}

export type CreateExportProfileDto = {
    name: string,
    description?: string,
    exportType?: string,
    customType?: string,
    customSource?: string,
    targetPath?: string
}

export type EditExportProfileDto = {
    name: string,
    description?: string,
    exportType?: string,
    customType?: string,
    customSource?: string,
    targetPath?: string
}