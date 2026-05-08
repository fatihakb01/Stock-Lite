using Domain.Entities.Core;
using Domain.Enums;

namespace Test.Helper
{
    public static class SchedulerHelper
    {
        internal static JobTask CreateJobTask(
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

        internal static List<JobTask> CreateJobTasks(int total = 5)
        {
            List<JobTask> tasks = new List<JobTask>();

            for (int i = 1; i <= total; i++)
            {
                string name = "Test Task " + i;
                string jobTaskKey = "TEST-" + i;
                JobTask task = CreateJobTask(name: name, jobTaskKey: jobTaskKey);
                tasks.Add(task);
            }

            return tasks;
        }

        internal static ScheduledJob CreateScheduledJob(
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

        internal static List<ScheduledJob> CreateScheduledJobs(JobTask jobTask, JobType jobType = JobType.Import, int total = 5)
        {
            List<ScheduledJob> jobs = new List<ScheduledJob>();

            for (int i = 1; i <= total; i++)
            {
                string name = "Test Job " + i;
                ScheduledJob job = CreateScheduledJob(name: name, executionOrder: i - 1, jobType: jobType, task: jobTask);

                jobs.Add(job);
            }

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
            var jobTask = task ?? CreateJobTask();
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
            var jobTask = task ?? CreateJobTask();
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
            var jobTask = task ?? CreateJobTask();
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

        internal static JobExecution CreateJobExecution(
            Guid jobTaskId,
            Guid scheduledJobId,
            JobTriggerType triggerType = JobTriggerType.Scheduled,
            JobExecutionStatus status = JobExecutionStatus.Running
        )
        {
            var jobTaskExecution = CreateJobTaskExecution(jobTaskId);

            return new JobExecution
            {
                ScheduledJobId = scheduledJobId,
                ScheduledJob = null!,
                JobTaskExecutionId = jobTaskExecution.Id,
                JobTaskExecution = jobTaskExecution,
                Status = status,
                TriggerType = triggerType,
                StartedAt = DateTime.UtcNow
            };
        }

        internal static (ScheduledJob job, JobTaskExecution taskExecution) CreateTaskAndExecution()
        {
            var jobTask = CreateJobTask();
            var job = CreateScheduledJob(task: jobTask);
            var taskExecution = CreateJobTaskExecution(jobTask.Id);
            return (job, taskExecution);
        }

        internal static List<JobExecution> CreateJobExecutions(Guid jobTaskId, List<ScheduledJob> jobs)
        {
            List<JobExecution> executions = new List<JobExecution>();

            foreach (var job in jobs)
            {
                var execution = CreateJobExecution(jobTaskId, job.Id);
                executions.Add(execution);
            }

            return executions;
        }
    }
}
