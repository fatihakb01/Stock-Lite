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
    private static IServiceScopeFactory CreateScopeFactory(
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

        var factory = new Mock<IServiceScopeFactory>();
        factory.Setup(x => x.CreateScope()).Returns(scope.Object);

        return factory.Object;
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
        runner.Setup(x => x.RunAsync(jobTask, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(JobExecutionStatus.Succeeded);

        var scopeFactory = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var worker = new SchedulerWorker(scopeFactory);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await worker.StartAsync(cts.Token);

        runner.Verify(x =>
            x.RunAsync(jobTask, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
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

        var scopeFactory = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var worker = new SchedulerWorker(scopeFactory);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await worker.StartAsync(cts.Token);

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

        var scopeFactory = CreateScopeFactory(companyRepo, jobTaskRepo, runner);

        var worker = new SchedulerWorker(scopeFactory);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await worker.StartAsync(cts.Token);

        runner.Verify(x =>
            x.RunAsync(It.IsAny<JobTask>(), It.IsAny<JobTriggerType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}