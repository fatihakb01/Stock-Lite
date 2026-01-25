export type ScheduledJob = {
    jobTaskKey: string,
    id: string,
    isArchived: boolean,
    createdAt: Date,
    createdBy: string,
    name: string,
    description: string,
    jobType: string,
    calculationType: string,
    importDataType: string,
    periodType: string,
    source: string,
    target: string,
    executionOrder: number
};

export type ScheduledJobExecution = {
  jobTaskName: string,
  scheduledJobName: string,
  status: number;          
  startedAt: string;
  finishedAt: string;
  errorMessage: string;
};

export type CreateScheduledJobDto = {
    jobTaskKey: string,
    name: string,
    description?: string,
    jobType: string,
    calculationType?: string,
    importDataType?: string,
    periodType?: string,
    source?: string,
    target?: string,
    executionOrder: number,
};

export type EditScheduledJobDto = {
    jobTaskKey: string,
    isArchived: boolean,
    name: string,
    description?: string,
    jobType: string,
    calculationType?: string,
    importDataType?: string,
    periodType?: string,
    source?: string,
    target?: string,
    executionOrder: number,
};
