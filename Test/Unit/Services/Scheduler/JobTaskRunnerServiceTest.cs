using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Infrastructure.Services.Scheduler;
using Moq;

namespace Test.Unit.Services.Scheduler
{
    public class JobTaskRunnerServiceTest
    {
        private readonly Mock<IJobTaskExecutionRepository> _executionRepoMock;
        private readonly Mock<IJobTaskDispatcher> _dispatcherMock;
        private readonly Mock<IProductKpiRepository> _productKpiRepoMock;
        private readonly JobTaskRunnerService _service;

        public JobTaskRunnerServiceTest()
        {
            _executionRepoMock = new Mock<IJobTaskExecutionRepository>();
            _dispatcherMock = new Mock<IJobTaskDispatcher>();
            _productKpiRepoMock = new Mock<IProductKpiRepository>();

            _service = new JobTaskRunnerService(
                _executionRepoMock.Object,
                _dispatcherMock.Object,
                _productKpiRepoMock.Object
            );
        }

        internal static JobTaskExecution CreateExecution(
            JobTask jobTask, JobTriggerType
            triggerType = JobTriggerType.Scheduled
        ) => new JobTaskExecution
        {
            Id = Guid.NewGuid(),
            JobTaskId = jobTask.Id,
            JobTask = jobTask,
            StartedAt = DateTime.UtcNow,
            Status = JobExecutionStatus.Running,
            TriggerType = triggerType
        };

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchSucceeds_SetsJobTaskResultToSucceeded(
            JobTriggerType triggerType, 
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            var result = await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, result);
            Assert.Equal(JobExecutionStatus.Succeeded, jobTask.Result);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchThrows_SetsJobTaskResultToFailed(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            var result = await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
            Assert.Equal(JobExecutionStatus.Failed, jobTask.Result);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchThrows_SetsErrorMessageOnExecution(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );
            JobTaskExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Something went wrong"));

            // Capture the execution object created internally by the service
            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobTaskExecution>()))
                .Callback<JobTaskExecution>(e => capturedExecution = e);

            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            Assert.NotNull(capturedExecution);
            Assert.Equal(JobExecutionStatus.Failed, capturedExecution!.Status);
            Assert.NotNull(capturedExecution.ErrorMessage);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchSucceeds_SetsExecutionStatusToSucceeded(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );
            JobTaskExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobTaskExecution>()))
                .Callback<JobTaskExecution>(e => capturedExecution = e);

            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            Assert.NotNull(capturedExecution);
            Assert.Equal(JobExecutionStatus.Succeeded, capturedExecution!.Status);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchSucceeds_SetsFinishedAt(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );
            JobTaskExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobTaskExecution>()))
                .Callback<JobTaskExecution>(e => capturedExecution = e);

            var before = DateTime.UtcNow;
            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution.FinishedAt.Value, before, after);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchThrows_StillSetsFinishedAt(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );
            JobTaskExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobTaskExecution>()))
                .Callback<JobTaskExecution>(e => capturedExecution = e);

            var before = DateTime.UtcNow;
            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);
            var after = DateTime.UtcNow;

            // FinishedAt must be set even when dispatch fails — it lives in the finally block
            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution!.FinishedAt!.Value, before, after);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_Always_CallsUpdateAndSaveOnExecutionRepository(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            _executionRepoMock.Verify(
                r => r.Update(It.IsAny<JobTaskExecution>()),
                Times.Once);

            _executionRepoMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.AtLeast(2)); // once after AddAsync, once in finally
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchSucceeds_CallsUpdateSchedulerStatus(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            _productKpiRepoMock.Verify(
                r => r.UpdateSchedulerStatus(jobTask.CompanyId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(JobTriggerType.Manual, true)]
        [InlineData(JobTriggerType.Scheduled, false)]
        [InlineData(JobTriggerType.Retry, false)]
        public async Task RunAsync_DispatchThrows_StillCallsUpdateSchedulerStatus(
            JobTriggerType triggerType,
            bool allowManualTriggerOnly
        )
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask(
                allowManualTriggerOnly: allowManualTriggerOnly,
                scheduledAt: allowManualTriggerOnly ? null : new DateTime(2026, 1, 1)
            );

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            await _service.RunAsync(jobTask, triggerType, CancellationToken.None);

            // UpdateSchedulerStatus lives in finally — must always be called
            _productKpiRepoMock.Verify(
                r => r.UpdateSchedulerStatus(jobTask.CompanyId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}