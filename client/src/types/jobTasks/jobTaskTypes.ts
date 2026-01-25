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
    jobs: {
        id: string,
        isArchived: boolean,
        createdAt: Date,
        createdBy: string,
        name: string,
        description: string,
        jobType: number,
        calculationType: number,
        importDataType: string,
        periodType: string,
        source: string,
        target: string,
        executionOrder: number
    }[]
};

export type JobTaskAndExecutions = {
    id: string,
    jobTaskKey: string,
    name: string,
    description: string,
    isArchived: boolean,
    jobs: {
        id: string,
        isArchived: boolean,
        createdAt: Date,
        createdBy: string,
        name: string,
        description: string,
        jobType: number,
        calculationType: number,
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
    }[]
};

// export type JobTaskExecutionHistory = {
//   jobTaskId: string;
//   jobTaskName: string;

//   jobs: {
//     jobId: string;
//     jobName: string;
//     executionOrder: number;

//     executions: JobExecution[];
//   }[];
// };

// export type JobTaskPipeline = {
//   id: string;
//   jobTaskKey: string;
//   name: string;
//   description?: string;
//   isArchived: boolean;

//   jobs: JobTaskJob[];
// };

// export type JobTaskListItem = {
//   id: string;
//   jobTaskKey: string;
//   name: string;
//   isArchived: boolean;

//   jobCount: number;
//   lastExecutionStatus?: number;
//   lastExecutionAt?: string;
// };

// export type JobTaskJob = {
//   id: string;
//   name: string;
//   executionOrder: number;

//   jobType: number;
//   calculationType?: number;
//   importDataType?: number;
//   periodType?: number;

//   isArchived: boolean;
// };

export type JobExecution = {
  status: number;          
  startedAt: string;
  finishedAt?: string;
  errorMessage?: string;
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
