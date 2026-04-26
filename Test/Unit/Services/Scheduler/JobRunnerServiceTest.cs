using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Infrastructure.Services.Scheduler;
using Moq;

namespace Test.Unit.Services.Scheduler
{
    public class JobRunnerServiceTest
    {
        private readonly Mock<IJobExecutionRepository> _executionRepoMock;
        private readonly Mock<IJobDispatcher> _dispatcherMock;
        private readonly JobRunnerService _service;

        public JobRunnerServiceTest()
        {
            _executionRepoMock = new Mock<IJobExecutionRepository>();
            _dispatcherMock = new Mock<IJobDispatcher>();

            _service = new JobRunnerService(
                _executionRepoMock.Object,
                _dispatcherMock.Object
            );
        }

        private static (ScheduledJob job, JobTaskExecution taskExecution) CreateJobAndExecution()
        {
            var jobTask = JobTaskDispatcherTest.CreateJobTask();
            var taskExecution = JobTaskRunnerServiceTest.CreateExecution(jobTask);
            var job = JobTaskDispatcherTest.CreateScheduledJob(jobTask.Id);
            return (job, taskExecution);
        }

        [Fact]
        public async Task RunAsync_DispatchSucceeds_SetsJobResultToSucceeded()
        {
            var (job, taskExecution) = CreateJobAndExecution();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.RunAsync(job, taskExecution, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Succeeded, result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsJobResultToFailed()
        {
            var (job, taskExecution) = CreateJobAndExecution();

            _dispatcherMock
                .Setup(d => d.DispatchAsync(job, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Dispatch failed"));

            var result = await _service.RunAsync(job, taskExecution, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
        }

        [Fact]
        public async Task RunAsync_DispatchThrows_SetsErrorMessageOnExecution()
        {
            var (job, taskExecution) = CreateJobAndExecution();
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
            var (job, taskExecution) = CreateJobAndExecution();
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
            var (job, taskExecution) = CreateJobAndExecution();
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
            var (job, taskExecution) = CreateJobAndExecution();

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
    }
}