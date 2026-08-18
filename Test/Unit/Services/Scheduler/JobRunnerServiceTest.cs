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
    public class JobRunnerServiceTest
    {
        private readonly Mock<IScheduledJobRepository> _scheduledJobRepoMock;
        private readonly Mock<IJobExecutionRepository> _executionRepoMock;
        private readonly Mock<IJobDispatcher> _dispatcherMock;
        private readonly Mock<ISchedulerCacheService> _cacheServiceMock;
        private readonly Mock<ICompanyNotificationService> _notifierMock;
        private readonly JobRunnerService _service;

        public JobRunnerServiceTest()
        {
            _scheduledJobRepoMock = new Mock<IScheduledJobRepository>();
            _executionRepoMock = new Mock<IJobExecutionRepository>();
            _dispatcherMock = new Mock<IJobDispatcher>();
            _cacheServiceMock = new Mock<ISchedulerCacheService>();
            _notifierMock = new Mock<ICompanyNotificationService>();

            _service = new JobRunnerService(
                _scheduledJobRepoMock.Object,
                _executionRepoMock.Object,
                _dispatcherMock.Object,
                _cacheServiceMock.Object,
                _notifierMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsJobResultToSucceeded()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            //_cacheServiceMock
            //    .Setup(c => c.RemoveAsync($"scheduledjob:{job.Id}", It.IsAny<CancellationToken>()))
            //    .Returns(Task.CompletedTask);

            var result = await _service.RunAsync(job, taskExecution, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsJobResultToFailed()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Dispatch failed"));

            var result = await _service.RunAsync(job, taskExecution, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsErrorMessageOnExecution()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();
            JobExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobExecution>()))
                .Callback<JobExecution>(e => capturedExecution = e);

            await _service.RunAsync(job, taskExecution, CancellationToken.None);

            Assert.NotNull(capturedExecution);
            Assert.Equal(JobExecutionStatus.Failed, capturedExecution!.Status);
            Assert.NotNull(capturedExecution.ErrorMessage);
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsFinishedAt()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();
            JobExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobExecution>()))
                .Callback<JobExecution>(e => capturedExecution = e);

            var before = DateTime.UtcNow;
            await _service.RunAsync(job, taskExecution, CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution!.FinishedAt!.Value, before, after);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_StillSetsFinishedAt()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();
            JobExecution? capturedExecution = null;

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Dispatch failed"));

            _executionRepoMock
                .Setup(r => r.Update(It.IsAny<JobExecution>()))
                .Callback<JobExecution>(e => capturedExecution = e);

            var before = DateTime.UtcNow;
            await _service.RunAsync(job, taskExecution, CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(capturedExecution?.FinishedAt);
            Assert.InRange(capturedExecution!.FinishedAt!.Value, before, after);
        }

        [Fact]
        public async Task RunAsync_Always_CallsUpdateAndSaveOnExecutionRepository()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Dispatch failed"));

            await _service.RunAsync(job, taskExecution, CancellationToken.None);

            _executionRepoMock.Verify(
                r => r.Update(It.IsAny<JobExecution>()),
                Times.Once);

            _executionRepoMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task RunAsync_Always_CallsRefetchScheduledJob()
        {
            var (_, job, taskExecution) = SchedulerHelper.CreateTaskAndExecutionStatic();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.RunAsync(job, taskExecution, CancellationToken.None);

            _scheduledJobRepoMock.Verify(
                r => r.SaveChangesAsync(CancellationToken.None),
                Times.Exactly(2));

            _cacheServiceMock.Verify(
                r => r.RemoveScheduledJobKeysAsync(job, CancellationToken.None),
                Times.Exactly(2));

            _notifierMock.Verify(
                r => r.NotifyScheduledJobUpdated(job.JobTask.CompanyId, job, CancellationToken.None),
                Times.Exactly(2));
        }
    }
}