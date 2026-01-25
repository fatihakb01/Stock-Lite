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

export type ScheduledJobAndExecutions = {
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
    executionOrder: number,
    jobExecutions: {
        status: number,
        startedAt: Date,
        finishedAt?: Date,
        errorMessage?: string
    }[]
};

// export type ScheduledJobExecutionHistory = {
//   scheduledJobId: string;
//   jobName: string;

//   executions: JobExecution[];
// };

// export type ScheduledJobListItem = {
//   id: string;
//   name: string;
//   executionOrder: number;
//   isArchived: boolean;

//   jobType: number;
//   lastExecutionStatus?: number;
// };

// export type ScheduledJobDetail = {
//   id: string;
//   name: string;
//   description?: string;
//   executionOrder: number;

//   jobType: number;
//   calculationType?: number;
//   importDataType?: number;
//   periodType?: number;

//   source?: string;
//   target?: string;

//   isArchived: boolean;
// };

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
