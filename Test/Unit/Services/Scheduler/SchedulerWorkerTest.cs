using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.Scheduler;

public class SchedulerWorkerTest
{
    private static IServiceScope CreateScopeFactory(
        Mock<ICompanyRepository> companyRepo,
        Mock<IJobTaskRepository> jobTaskRepo,
        Mock<IJobTaskRunnerService> runnerService,
        Mock<IProductKpiRepository> productKpiRepo,
        Mock<ISchedulerCacheService> schedulerCacheService,
        Mock<ICompanyNotificationService> schedulerNotificationService)
    {
        var scopedProvider = new Mock<IServiceProvider>();

        scopedProvider.Setup(x => x.GetService(typeof(ICompanyRepository)))
            .Returns(companyRepo.Object);

        scopedProvider.Setup(x => x.GetService(typeof(IJobTaskRepository)))
            .Returns(jobTaskRepo.Object);

        scopedProvider.Setup(x => x.GetService(typeof(IJobTaskRunnerService)))
            .Returns(runnerService.Object);

        scopedProvider.Setup(x => x.GetService(typeof(IProductKpiRepository)))
            .Returns(productKpiRepo.Object);

        scopedProvider.Setup(x => x.GetService(typeof(ISchedulerCacheService)))
            .Returns(schedulerCacheService.Object);

        scopedProvider.Setup(x => x.GetService(typeof(ICompanyNotificationService)))
            .Returns(schedulerNotificationService.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(x => x.ServiceProvider).Returns(scopedProvider.Object);

        return scope.Object;
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskAvailable_RunsTask()
    {
        var companyId = Guid.NewGuid();
        var jobTask = SchedulerHelper.CreateJobTaskStatic();
        jobTask.CompanyId = companyId;

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask);

        var runner = new Mock<IJobTaskRunnerService>();
        var productKpiRepo = new Mock<IProductKpiRepository>();
        var schedulerCache = new Mock<ISchedulerCacheService>();
        var schedulerNotification = new Mock<ICompanyNotificationService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner, productKpiRepo, schedulerCache, schedulerNotification);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(jobTask, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()),
            Times.Once);

        productKpiRepo.Verify(x =>
            x.GetSchedulerStatus(companyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleCompaniesAvailable_RunsTaskMultipleTimes()
    {
        var companyId1 = Guid.NewGuid();
        var jobTask1 = SchedulerHelper.CreateJobTaskStatic();
        jobTask1.CompanyId = companyId1;

        var companyId2 = Guid.NewGuid();
        var jobTask2 = SchedulerHelper.CreateJobTaskStatic();
        jobTask2.CompanyId = companyId2;

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId1, companyId2 });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask1);

        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId2, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask2);

        var runner = new Mock<IJobTaskRunnerService>();
        var productKpiRepo = new Mock<IProductKpiRepository>();
        var schedulerCache = new Mock<ISchedulerCacheService>();
        var schedulerNotification = new Mock<ICompanyNotificationService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner, productKpiRepo, schedulerCache, schedulerNotification);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(jobTask1, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()),
            Times.Once);

        productKpiRepo.Verify(x =>
            x.GetSchedulerStatus(companyId1, It.IsAny<CancellationToken>()),
            Times.Once);

        runner.Verify(x =>
            x.RunAsync(jobTask2, JobTriggerType.Scheduled, It.IsAny<CancellationToken>()),
            Times.Once);

        productKpiRepo.Verify(x =>
            x.GetSchedulerStatus(companyId2, It.IsAny<CancellationToken>()),
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
        var productKpiRepo = new Mock<IProductKpiRepository>();
        var schedulerCache = new Mock<ISchedulerCacheService>();
        var schedulerNotification = new Mock<ICompanyNotificationService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner, productKpiRepo, schedulerCache, schedulerNotification);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        runner.Verify(x =>
            x.RunAsync(It.IsAny<JobTask>(), It.IsAny<JobTriggerType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(JobExecutionStatus.Pending, 1)]
    [InlineData(JobExecutionStatus.Succeeded, 0)]
    [InlineData(JobExecutionStatus.Failed, 0)]
    public async Task ExecuteAsync_WhenSchedulerStatus_NotifySchedulerFinished(JobExecutionStatus status, int numberOfTimes)
    {
        var companyId = Guid.NewGuid();
        var jobTasks = SchedulerHelper.CreateJobTasksStatic();
        jobTasks.ForEach(x => x.CompanyId = companyId);
        var jobTask = jobTasks[0];

        var companyRepo = new Mock<ICompanyRepository>();
        companyRepo.Setup(x => x.GetCompanyIds(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { companyId });

        var jobTaskRepo = new Mock<IJobTaskRepository>();
        jobTaskRepo.Setup(x => x.HasExhaustedTaskAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        jobTaskRepo.Setup(x => x.GetNextDueTaskAsync(companyId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTask);

        var runner = new Mock<IJobTaskRunnerService>();
        var productKpiRepo = new Mock<IProductKpiRepository>();
        productKpiRepo.Setup(x => x.GetSchedulerStatus(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        jobTaskRepo.Setup(x => x.FindAsync(x => x.CompanyId == companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobTasks);

        var cacheService = new Mock<ISchedulerCacheService>();
        var schedulerNotification = new Mock<ICompanyNotificationService>();
        var scope = CreateScopeFactory(companyRepo, jobTaskRepo, runner, productKpiRepo, cacheService, schedulerNotification);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await SchedulerWorker.ExecuteOnceAsync(scope.ServiceProvider, cts.Token);

        cacheService.Verify(x => x.RemoveSchedulerKeys(jobTasks, It.IsAny<CancellationToken>()), Times.Exactly(numberOfTimes));
        schedulerNotification.Verify(x => x.NotifySchedulerFinished(jobTasks, It.IsAny<CancellationToken>()), Times.Exactly(numberOfTimes));
    }
}