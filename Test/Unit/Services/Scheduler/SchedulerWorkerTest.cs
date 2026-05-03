using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Infrastructure.Services.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Test.Unit.Services.Scheduler;

public class SchedulerWorkerTest
{
    private static IServiceScope CreateScopeFactory(
        Mock<ICompanyRepository> companyRepo,
        Mock<IJobTaskRepository> jobTaskRepo,
        Mock<IJobTaskRunnerService> runnerService)
    {
        var scopedProvider = new Mock<IServiceProvider>();

        scopedProvider.Setup(x => x.GetService(typeof(ICompanyRepository)))
            .Returns(companyRepo.Object);

        scopedProvider.Setup(x => x.GetService(typeof(IJobTaskRepository)))
            .Returns(jobTaskRepo.Object);

        scopedProvider.Setup(x => x.GetService(typeof(IJobTaskRunnerService)))
            .Returns(runnerService.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(x => x.ServiceProvider).Returns(scopedProvider.Object);

        return scope.Object;
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskAvailable_RunsTask()
    {
        var companyId = Guid.NewGuid();

        var jobTask = JobTaskDispatcherTest.CreateJobTask();
        jobTask.CompanyId = companyId;

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask);

        jobTaskRepo.Setup(x => x.TryLockTaskAsync(jobTask.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var runner = new Mock<IJobTaskRunnerService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(jobTask, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskIsBlocked_DoesNotRun()
    {
        var companyId = Guid.NewGuid();

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var runner = new Mock<IJobTaskRunnerService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(It.IsAny<JobTask>(), It.IsAny<JobTriggerType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLockFails_DoesNotRun()
    {
        var companyId = Guid.NewGuid();

        var jobTask = JobTaskDispatcherTest.CreateJobTask();
        jobTask.CompanyId = companyId;

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask);

        jobTaskRepo.Setup(x => x.TryLockTaskAsync(jobTask.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var runner = new Mock<IJobTaskRunnerService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(It.IsAny<JobTask>(), It.IsAny<JobTriggerType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}