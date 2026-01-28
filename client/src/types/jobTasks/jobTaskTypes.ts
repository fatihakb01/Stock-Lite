import type { ScheduledJob } from "../scheduledJobs/scheduledJobTypes";

export type JobTask = {
    id: string,
    jobTaskKey: string,
    name: string,
    description: string,
    isArchived: boolean,
    jobs: {
        id: string,
        isArchived: boolean,
        jobType: number,
        calculationType: number,
        importDataType: string,
        periodType: string,
        executionOrder: number
    }[]
};

export type JobTaskDetail = {
    id: string,
    jobTaskKey: string,
    name: string,
    description: string,
    isArchived: boolean,
    jobs: ScheduledJob[]
};

export type JobTaskExecution = {
  jobTaskName: string,
  scheduledJobName: string,
  status: number;          
  startedAt: string;
  finishedAt: string;
  errorMessage: string;
};

export type CreateJobTaskDto = {
    jobTaskKey: string,
    name: string,
    description?: string
};

export type EditJobTaskDto = {
    jobTaskKey: string,
    isArchived: boolean,
    name: string,
    description?: string
};
