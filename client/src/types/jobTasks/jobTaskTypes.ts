import type { ScheduledJob } from "../scheduledJobs/scheduledJobTypes";

export type JobTask = {
    id: string,
    name: string
};

export type JobTaskDetail = {
    id: string,
    jobTaskKey: string,
    name: string,
    description: string,
    scheduledAt?: string,
    nextRunAt?: string,
    periodType?: string,
    maxRetries?: number,
    retryDelayMinutes?: number,
    isArchived: boolean,
    allowManualTriggerOnly: boolean,
    jobs: ScheduledJob[]
};

export type JobTaskExecution = {
  jobTaskName: string,
  scheduledJobName: string,
  triggerType: number,
  status: number;          
  startedAt: string;
  finishedAt?: string;
  errorMessage?: string;
};

export type CreateJobTaskDto = {
    jobTaskKey: string,
    name: string,
    description?: string,
    scheduledAt?: string,
    periodType?: string,
    maxRetries?: number,
    retryDelayMinutes?: number
};

export type EditJobTaskDto = {
    jobTaskKey: string,
    isArchived: boolean,
    name: string,
    description?: string,
    scheduledAt?: string,
    nextRunAt?: string,
    periodType?: string,
    maxRetries?: number,
    retryDelayMinutes?: number
};
