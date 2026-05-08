using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Infrastructure.Services.Scheduler;
using Moq;
using System.Linq.Expressions;
using Test.Helper;

namespace Test.Unit.Services.Scheduler
{
    public class JobTaskDispatcherTest
    {
        private readonly Mock<IJobTaskRepository> _jobTaskRepoMock;
        private readonly Mock<IScheduledJobRepository> _scheduledJobRepoMock;
        private readonly Mock<IJobRunnerService> _jobRunnerServiceMock;
        private readonly JobTaskDispatcher _dispatcher;

        public JobTaskDispatcherTest()
        {
            _jobTaskRepoMock = new Mock<IJobTaskRepository>();
            _scheduledJobRepoMock = new Mock<IScheduledJobRepository>();
            _jobRunnerServiceMock = new Mock<IJobRunnerService>();

            _dispatcher = new JobTaskDispatcher(
                _jobTaskRepoMock.Object,
                _scheduledJobRepoMock.Object,
                _jobRunnerServiceMock.Object
            );
        }

        [Fact]
        public async Task DispatchAsync_AllJobsSucceed_SetsJobTaskResultToSucceeded()
        {
            var jobTask = SchedulerHelper.CreateJobTask();
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Succeeded);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, jobTask.Result);
            Assert.Equal(0, jobTask.Retries);
        }

        [Theory]
        [InlineData(JobExecutionStatus.Failed, JobExecutionStatus.Pending)]
        [InlineData(JobExecutionStatus.Succeeded, JobExecutionStatus.Succeeded)]
        public async Task DispatchAsync_FirstJobResult_SetsJobResult(
            JobExecutionStatus statusJob1,
            JobExecutionStatus statusJob2
        )
        {
            var jobTask = SchedulerHelper.CreateJobTask();
            var job1 = SchedulerHelper.CreateScheduledJob(executionOrder: 0, task: jobTask);
            var job2 = SchedulerHelper.CreateScheduledJob(executionOrder: 1, task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job1, job2 });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job1, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(statusJob1);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(statusJob1, job1.Result);
            Assert.Equal(statusJob2, job2.Result);
        }

        [Fact]
        public async Task DispatchAsync_AllJobsSucceed_CallsResetScheduler()
        {
            var jobTask = SchedulerHelper.CreateJobTask();
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Succeeded);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            _jobTaskRepoMock.Verify(
                r => r.ResetScheduler(jobTask.CompanyId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task DispatchAsync_AllJobsSucceed_ScheduledTask_UpdatesNextRunAt()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTask(allowManualTriggerOnly: false, periodType: Period.Daily, scheduledAt: scheduledAt);
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Succeeded);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(scheduledAt.AddDays(1), jobTask.NextRunAt);
            Assert.Equal(scheduledAt.AddDays(1), jobTask.ScheduledAt);
        }

        [Fact]
        public async Task DispatchAsync_AllJobsSucceed_ManualTriggerOnly_DoesNotUpdateNextRunAt()
        {
            var jobTask = SchedulerHelper.CreateJobTask(allowManualTriggerOnly: true);
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Succeeded);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Null(jobTask.NextRunAt);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(null, 1)]
        public async Task DispatchAsync_JobFails_WithRetriesRemaining_SetsNextRunAtUsingRetryDelay(int? retryDelayMinutes, int expectedDelayMinutes)
        {
            var jobTask = SchedulerHelper.CreateJobTask(retries: 0, maxRetries: 3, retryDelayMinutes: retryDelayMinutes);
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Failed);

            var before = DateTime.UtcNow;
            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(jobTask.NextRunAt);
            Assert.InRange(
                jobTask.NextRunAt.Value,
                before.AddMinutes(expectedDelayMinutes),
                after.AddMinutes(expectedDelayMinutes)
            );
        }

        [Fact]
        public async Task DispatchAsync_JobFails_WithRetriesRemaining_IncrementsRetriesAndSetsNextRunAt()
        {
            var jobTask = SchedulerHelper.CreateJobTask(retries: 0, maxRetries: 3);
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Failed);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, jobTask.Result);
            Assert.Equal(1, jobTask.Retries);
            Assert.NotNull(jobTask.NextRunAt);
        }

        [Fact]
        public async Task DispatchAsync_JobFails_MaxRetriesReached_DoesNotSetNextRunAt()
        {
            var jobTask = SchedulerHelper.CreateJobTask(retries: 3, maxRetries: 3);
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Failed);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, jobTask.Result);
            Assert.Equal(4, jobTask.Retries);
            Assert.Null(jobTask.NextRunAt);
        }

        [Fact]
        public async Task DispatchAsync_FirstJobFails_SecondJobIsNotExecuted()
        {
            var jobTask = SchedulerHelper.CreateJobTask();
            var job1 = SchedulerHelper.CreateScheduledJob(executionOrder: 0, task: jobTask);
            var job2 = SchedulerHelper.CreateScheduledJob(executionOrder: 1, task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job1, job2 });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _jobRunnerServiceMock
                .Setup(r => r.RunAsync(job1, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JobExecutionStatus.Failed);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            _jobRunnerServiceMock.Verify(
                r => r.RunAsync(job2, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DispatchAsync_LockFails_JobIsSkipped()
        {
            var jobTask = SchedulerHelper.CreateJobTask();
            var job = SchedulerHelper.CreateScheduledJob(task: jobTask);

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob> { job });

            _scheduledJobRepoMock
                .Setup(r => r.TryLockTaskAsync(job.Id))
                .ReturnsAsync(false);

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            _jobRunnerServiceMock.Verify(
                r => r.RunAsync(It.IsAny<ScheduledJob>(), It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DispatchAsync_NoJobs_SetsJobTaskResultToSucceeded()
        {
            var jobTask = SchedulerHelper.CreateJobTask();

            _scheduledJobRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ScheduledJob, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScheduledJob>());

            await _dispatcher.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, jobTask.Result);
        }
    }
}