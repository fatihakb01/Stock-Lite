using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.Scheduler;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.Scheduler
{
    public class JobTaskRunnerServiceTest
    {
        private readonly Mock<IJobTaskRepository> _jobTaskRepoMock;
        private readonly Mock<IJobTaskExecutionRepository> _executionRepoMock;
        private readonly Mock<IJobTaskDispatcher> _dispatcherMock;
        private readonly Mock<ISchedulerCacheService> _cacheServiceMock;
        private readonly Mock<IProductKpiRepository> _productKpiRepoMock;
        private readonly Mock<ISchedulerNotificationService> _notificationServiceMock;
        private readonly JobTaskRunnerService _service;

        public JobTaskRunnerServiceTest()
        {
            _jobTaskRepoMock = new Mock<IJobTaskRepository>();
            _executionRepoMock = new Mock<IJobTaskExecutionRepository>();
            _dispatcherMock = new Mock<IJobTaskDispatcher>();
            _productKpiRepoMock = new Mock<IProductKpiRepository>();
            _cacheServiceMock = new Mock<ISchedulerCacheService>();
            _notificationServiceMock = new Mock<ISchedulerNotificationService>();

            _service = new JobTaskRunnerService(
                _jobTaskRepoMock.Object,
                _executionRepoMock.Object,
                _dispatcherMock.Object,
                _productKpiRepoMock.Object,
                _cacheServiceMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsJobTaskResultToSucceeded()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            var result = await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, result);
            Assert.Equal(JobExecutionStatus.Succeeded, jobTask.Result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsJobTaskResultToFailed()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            var result = await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
            Assert.Equal(JobExecutionStatus.Failed, jobTask.Result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsErrorMessageOnExecution()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);
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

            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            Assert.NotNull(capturedExecution);
            Assert.Equal(JobExecutionStatus.Failed, capturedExecution!.Status);
            Assert.NotNull(capturedExecution.ErrorMessage);
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsExecutionStatusToSucceeded()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);
            JobTaskExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobTaskExecution>()))
                .Callback<JobTaskExecution>(e => capturedExecution = e);

            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            Assert.NotNull(capturedExecution);
            Assert.Equal(JobExecutionStatus.Succeeded, capturedExecution!.Status);
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsFinishedAt()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);
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
            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution.FinishedAt.Value, before, after);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_StillSetsFinishedAt()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);
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
            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);
            var after = DateTime.UtcNow;

            // FinishedAt must be set even when dispatch fails — it lives in the finally block
            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution!.FinishedAt!.Value, before, after);
        }

        [Fact]
        public async Task RunAsync_Always_CallsUpdateAndSaveOnExecutionRepository()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            _executionRepoMock.Verify(
                r => r.Update(It.IsAny<JobTaskExecution>()),
                Times.Once);

            _executionRepoMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(2)); // once after AddAsync, once in finally
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_CallsUpdateSchedulerStatus()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            _productKpiRepoMock.Verify(
                r => r.UpdateSchedulerStatus(jobTask.CompanyId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_StillCallsUpdateSchedulerStatus()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Failed)
                .ThrowsAsync(new Exception("Dispatch failed"));

            await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            // UpdateSchedulerStatus lives in finally — must always be called
            _productKpiRepoMock.Verify(
                r => r.UpdateSchedulerStatus(jobTask.CompanyId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_CallsRefetchJobTaskTwice()
        {
            var scheduledAt = new DateTime(2026, 1, 1);
            var jobTask = SchedulerHelper.CreateJobTaskStatic(scheduledAt: scheduledAt);

            _dispatcherMock
                .Setup(d => d.DispatchAsync(jobTask, It.IsAny<JobTaskExecution>(), It.IsAny<CancellationToken>()))
                .Callback<JobTask, JobTaskExecution, CancellationToken>((jt, _, _) =>
                    jt.Result = JobExecutionStatus.Succeeded)
                .Returns(Task.CompletedTask);

            var result = await _service.RunAsync(jobTask, JobTriggerType.Scheduled, CancellationToken.None);

            _jobTaskRepoMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Exactly(2));
            _cacheServiceMock.Verify(r => r.RemoveJobTaskKeysAsync(jobTask, CancellationToken.None), Times.Exactly(2));
            _notificationServiceMock.Verify(r => r.NotifyJobTaskUpdated(jobTask.CompanyId, jobTask.Id, CancellationToken.None), Times.Exactly(2));
        }
    }
}