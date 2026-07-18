using Domain.Entities.Core;
using Domain.Enums;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class SchedulerHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal async Task<JobTask> CreateJobTask(
            string name = "Test Task",
            string jobTaskKey = "TEST-",
            int retries = 0,
            int maxRetries = 3,
            int? retryDelayMinutes = 5,
            bool allowManualTriggerOnly = false,
            Period? periodType = Period.Daily,
            DateTime? scheduledAt = null,
            Guid? companyId = null,
            bool saveChanges = true
        )
        {
            var task = CreateJobTaskStatic(
                name: name,
                jobTaskKey: jobTaskKey,
                retries: retries,
                maxRetries: maxRetries,
                retryDelayMinutes: retryDelayMinutes,
                allowManualTriggerOnly: allowManualTriggerOnly,
                periodType: periodType,
                scheduledAt: scheduledAt,
                companyId: companyId
            );

            await _fixture.Context.JobTasks.AddAsync(task);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return task;
        }

        internal static JobTask CreateJobTaskStatic(
            string name = "Test Task",
            string jobTaskKey = "TEST-",
            int retries = 0,
            int maxRetries = 3,
            int? retryDelayMinutes = 5,
            bool allowManualTriggerOnly = false,
            Period? periodType = Period.Daily,
            DateTime? scheduledAt = null,
            Guid? companyId = null
        ) => new JobTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            JobTaskKey = jobTaskKey,
            Retries = retries,
            MaxRetries = maxRetries,
            RetryDelayMinutes = retryDelayMinutes,
            AllowManualTriggerOnly = allowManualTriggerOnly,
            PeriodType = periodType,
            ScheduledAt = scheduledAt,
            NextRunAt = scheduledAt,
            CompanyId = companyId ?? Guid.NewGuid(),
            Company = null!
        };

        internal static List<JobTask> CreateJobTasksStatic(int total = 5)
        {
            List<JobTask> tasks = new List<JobTask>();

            for (int i = 1; i <= total; i++)
            {
                string name = "Test Task " + i;
                string jobTaskKey = "TEST-" + i;
                JobTask task = CreateJobTaskStatic(name: name, jobTaskKey: jobTaskKey);
                tasks.Add(task);
            }

            return tasks;
        }

        internal async Task<List<JobTask>> CreateJobTasks(int total = 5, bool saveChanges = true)
        {
            List<JobTask> tasks = CreateJobTasksStatic(total);

            await _fixture.Context.JobTasks.AddRangeAsync(tasks);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return tasks;
        }

        internal static ScheduledJob CreateScheduledJobStatic(
            string name = "Test Job",
            int executionOrder = 0,
            JobType jobType = JobType.Import,
            JobTask? task = null
        )
        {
            return jobType switch
            {
                JobType.Import => CreateImportJob(executionOrder: executionOrder, name: name, task: task),
                JobType.StandardCalculation => CreateCalculationJob(executionOrder: executionOrder, name: name, task: task),
                JobType.CustomCalculation => CreateCustomJob(executionOrder: executionOrder, name: name, task: task),
                _ => throw new InvalidOperationException("Invalid JobType")
            };
        }

        internal async Task<ScheduledJob> CreateScheduledJob(
            string name = "Test Job", 
            int executionOrder = 0, 
            JobType jobType = JobType.Import,
            JobTask? task = null,
            bool saveChanges = true
        )
        {
            var job = CreateScheduledJobStatic(
                name: name,
                executionOrder: executionOrder,
                jobType: jobType,
                task: task
            );

            await _fixture.Context.ScheduledJobs.AddAsync(job);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return job;
        }

        internal async Task<List<ScheduledJob>> CreateScheduledJobs(JobTask jobTask, JobType jobType = JobType.Import, int total = 5)
        {
            List<ScheduledJob> jobs = new List<ScheduledJob>();

            for (int i = 1; i <= total; i++)
            {
                string name = "Test Job " + i;
                ScheduledJob job = CreateScheduledJobStatic(name: name, executionOrder: i - 1, jobType: jobType, task: jobTask);

                jobs.Add(job);
            }

            await _fixture.Context.ScheduledJobs.AddRangeAsync(jobs);
            await _fixture.Context.SaveChangesAsync();

            return jobs;
        }

        private static ScheduledJob CreateImportJob(
            int executionOrder = 0,
            string name = "Test Import Job",
            ImportDataType? dataType = ImportDataType.Products,
            string? source = "C:\\Data\\products.csv",
            JobTask? task = null
        )
        {
            var jobTask = task ?? CreateJobTaskStatic();
            return new ScheduledJob
            {
                Id = Guid.NewGuid(),
                Name = name,
                JobTaskId = jobTask.Id,
                JobType = JobType.Import,
                ImportDataType = dataType,
                Source = source,
                ExecutionOrder = executionOrder,
                IsArchived = false,
                Result = JobExecutionStatus.Pending,
                JobTask = jobTask
            };
        }

        private static ScheduledJob CreateCalculationJob(
            int executionOrder = 0,
            string name = "Test Calculation Job",
            StandardCalculationType calculationType = StandardCalculationType.StockLevels,
            Period? periodType = null,
            JobTask? task = null
        )
        {
            var jobTask = task ?? CreateJobTaskStatic();
            return new ScheduledJob
            {
                Id = Guid.NewGuid(),
                Name = name,
                JobTaskId = jobTask.Id,
                JobType = JobType.StandardCalculation,
                CalculationType = calculationType,
                PeriodType = periodType,
                ExecutionOrder = executionOrder,
                IsArchived = false,
                Result = JobExecutionStatus.Pending,
                JobTask = jobTask
            };
        }

        private static ScheduledJob CreateCustomJob(
            int executionOrder = 0,
            string name = "Test Custom Job",
            string? target = "custom_my_proc",
            JobTask? task = null
        )
        {
            var jobTask = task ?? CreateJobTaskStatic();
            return new ScheduledJob
            {
                Id = Guid.NewGuid(),
                Name = name,
                JobTaskId = jobTask.Id,
                JobType = JobType.CustomCalculation,
                Target = target,
                ExecutionOrder = executionOrder,
                IsArchived = false,
                Result = JobExecutionStatus.Pending,
                JobTask = jobTask
            };
        }

        internal static JobTaskExecution CreateJobTaskExecution(
            Guid jobTaskId,
            JobTriggerType triggerType = JobTriggerType.Scheduled,
            JobExecutionStatus status = JobExecutionStatus.Running
        ) => new JobTaskExecution
        {
            JobTaskId = jobTaskId,
            JobTask = null!,
            Status = status,
            TriggerType = triggerType,
            StartedAt = DateTime.UtcNow,
            JobExecutions = []
        };

        internal static JobExecution CreateJobExecutionStatic(
            Guid jobTaskId,
            Guid scheduledJobId,
            JobTriggerType triggerType = JobTriggerType.Scheduled,
            JobExecutionStatus status = JobExecutionStatus.Running,
            JobTaskExecution? jobTaskExecution = null
        )
        {
            var taskExecution = jobTaskExecution ?? CreateJobTaskExecution(jobTaskId);

            return new JobExecution
            {
                ScheduledJobId = scheduledJobId,
                ScheduledJob = null!,
                JobTaskExecutionId = taskExecution.Id,
                JobTaskExecution = taskExecution,
                Status = status,
                TriggerType = triggerType,
                StartedAt = DateTime.UtcNow
            };
        }

        internal async Task<JobExecution> CreateJobExecution(
            Guid jobTaskId,
            Guid scheduledJobId,
            JobTriggerType triggerType = JobTriggerType.Scheduled,
            JobExecutionStatus status = JobExecutionStatus.Running,
            JobTaskExecution? jobTaskExecution = null,
            bool saveChanges = true
        )
        {
            var execution = CreateJobExecutionStatic(
                jobTaskId: jobTaskId,
                scheduledJobId: scheduledJobId,
                triggerType: triggerType,
                status: status,
                jobTaskExecution: jobTaskExecution
            );

            await _fixture.Context.JobExecutions.AddAsync(execution);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return execution;
        }

        internal static (JobTask task, ScheduledJob job, JobTaskExecution taskExecution) CreateTaskAndExecutionStatic()
        {
            var task = CreateJobTaskStatic();
            var job = CreateScheduledJobStatic(task: task);
            var taskExecution = CreateJobTaskExecution(task.Id);

            return (task, job, taskExecution);
        }

        internal async Task<(JobTask task, ScheduledJob job, JobTaskExecution taskExecution)> CreateTaskAndExecution(bool saveChanges = true)
        {
            var (task, job, taskExecution) = CreateTaskAndExecutionStatic();

            await _fixture.Context.ScheduledJobs.AddAsync(job);
            await _fixture.Context.JobTasks.AddAsync(task);
            await _fixture.Context.JobTaskExecutions.AddAsync(taskExecution);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return (task, job, taskExecution);
        }

        internal async Task<List<JobExecution>> CreateJobExecutions(Guid jobTaskId, List<ScheduledJob> jobs)
        {
            List<JobExecution> executions = new List<JobExecution>();

            foreach (var job in jobs)
            {
                var execution = CreateJobExecutionStatic(jobTaskId, job.Id);
                executions.Add(execution);
            }

            await _fixture.Context.JobExecutions.AddRangeAsync(executions);
            await _fixture.Context.SaveChangesAsync();

            return executions;
        }
    }
}
