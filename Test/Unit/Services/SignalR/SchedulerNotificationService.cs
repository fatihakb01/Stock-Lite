using Domain.Entities.Core;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.SignalR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.SignalR
{
    public class SchedulerNotificationServiceTest
    {
    //    private readonly Mock<IHubClients> _hubClientsMock;
    //    private readonly Mock<IClientProxy> _clientProxyMock;
    //    private readonly Mock<INotificationService> _notificationServiceMock;
    //    private readonly CompanyNotificationService _notificationService;
    //    private readonly CancellationToken _cancellationToken;

    //    public SchedulerNotificationServiceTest()
    //    {
    //        _hubClientsMock = new Mock<IHubClients>();
    //        _clientProxyMock = new Mock<IClientProxy>();
    //        _notificationServiceMock = new Mock<INotificationService>();

    //        // Setup the Group method to return the client proxy mock
    //        _hubClientsMock
    //            .Setup(x => x.Group(It.IsAny<string>()))
    //            .Returns(_clientProxyMock.Object);

    //        _notificationService = new CompanyNotificationService(_notificationServiceMock.Object);
    //        _cancellationToken = CancellationToken.None;
    //    }

    //    [Fact]
    //    public async Task NotifyJobTaskUpdated_SendsJobTaskUpdatedMessageToCompanyGroup()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTaskId = Guid.NewGuid();
    //        var expectedGroup = $"company-{companyId}";

    //        // Act
    //        await _notificationService.NotifyJobTaskUpdated(companyId, jobTaskId, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group(expectedGroup), Times.Once);

    //        // Verify the SendCoreAsync method instead of SendAsync
    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTaskId),
    //                _cancellationToken),
    //            Times.Once);
    //    }

    //    [Fact]
    //    public async Task NotifyJobTaskUpdated_WithDifferentCompanyIds_SendsToCorrectGroups()
    //    {
    //        // Arrange
    //        var companyId1 = Guid.NewGuid();
    //        var companyId2 = Guid.NewGuid();
    //        var jobTaskId1 = Guid.NewGuid();
    //        var jobTaskId2 = Guid.NewGuid();

    //        // Act
    //        await _notificationService.NotifyJobTaskUpdated(companyId1, jobTaskId1, _cancellationToken);
    //        await _notificationService.NotifyJobTaskUpdated(companyId2, jobTaskId2, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId1}"), Times.Once);
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId2}"), Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTaskId1),
    //                _cancellationToken),
    //            Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTaskId2),
    //                _cancellationToken),
    //            Times.Once);
    //    }

    //    [Fact]
    //    public async Task NotifyJobTaskUpdated_WithSameCompanyDifferentJobTasks_SendsToSameGroup()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTaskId1 = Guid.NewGuid();
    //        var jobTaskId2 = Guid.NewGuid();

    //        // Act
    //        await _notificationService.NotifyJobTaskUpdated(companyId, jobTaskId1, _cancellationToken);
    //        await _notificationService.NotifyJobTaskUpdated(companyId, jobTaskId2, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId}"), Times.Exactly(2));

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTaskId1),
    //                _cancellationToken),
    //            Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTaskId2),
    //                _cancellationToken),
    //            Times.Once);
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_WithMultipleJobTasks_NotifiesAllJobsAndTasks()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTask1 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        var jobTask2 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        jobTask1.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask1, total: 2);
    //        jobTask2.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask2, total: 3);
    //        var jobTasks = new List<JobTask> { jobTask1, jobTask2 };

    //        // Act
    //        await _notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId}"), Times.Exactly(7)); // 2 JobTasks + 5 ScheduledJobs

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask1.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask2.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        foreach (var job in jobTask1.Jobs)
    //        {
    //            _clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }

    //        foreach (var job in jobTask2.Jobs)
    //        {
    //            _clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_WithSingleJobTaskAndNoJobs_NotifiesOnlyJobTask()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        var jobTasks = new List<JobTask> { jobTask };

    //        // Act
    //        await _notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId}"), Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "ScheduledJobUpdated",
    //                It.IsAny<object[]>(),
    //                _cancellationToken),
    //            Times.Never);
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_WithMultipleCompanies_NotifiesToCorrectGroups()
    //    {
    //        // Arrange
    //        var companyId1 = Guid.NewGuid();
    //        var companyId2 = Guid.NewGuid();
    //        var jobTask1 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId1);
    //        var jobTask2 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId2);
    //        jobTask1.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask1, total: 1);
    //        jobTask2.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask2, total: 1);
    //        var jobTasks = new List<JobTask> { jobTask1, jobTask2 };

    //        // Act
    //        await _notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId1}"), Times.Exactly(2));
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId2}"), Times.Exactly(2));

    //        // Verify JobTask notifications
    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask1.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask2.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        // Verify ScheduledJob notifications
    //        foreach (var job in jobTask1.Jobs)
    //        {
    //            _clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }

    //        foreach (var job in jobTask2.Jobs)
    //        {
    //            _clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_WithEmptyJobTaskList_DoesNotNotifyAnything()
    //    {
    //        // Arrange
    //        var jobTasks = new List<JobTask>();

    //        // Act
    //        await _notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), _cancellationToken),
    //            Times.Never);
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_WithJobTaskHavingMultipleJobs_NotifiesAllJobs()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        jobTask.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask, total: 5);
    //        var jobTasks = new List<JobTask> { jobTask };

    //        // Act
    //        await _notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert
    //        _hubClientsMock.Verify(x => x.Group($"company-{companyId}"), Times.Exactly(6));

    //        _clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        foreach (var job in jobTask.Jobs)
    //        {
    //            _clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }
    //    }

    //    [Fact]
    //    public async Task NotifySchedulerFinished_CallsNotifyJobTaskUpdatedAndNotifyScheduledJobUpdated_ForEachJobTask()
    //    {
    //        // Arrange
    //        var companyId = Guid.NewGuid();
    //        var jobTask1 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        var jobTask2 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
    //        jobTask1.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask1, total: 2);
    //        jobTask2.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask2, total: 1);
    //        var jobTasks = new List<JobTask> { jobTask1, jobTask2 };

    //        // Create separate mocks to verify the flow
    //        var hubClientsMock = new Mock<IHubClients>();
    //        var clientProxyMock = new Mock<IClientProxy>();
    //        hubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

    //        var notificationService = new CompanyNotificationService(_notificationServiceMock.Object);

    //        // Act
    //        await notificationService.NotifySchedulerFinished(jobTasks, _cancellationToken);

    //        // Assert - Verify all job tasks and their jobs were notified
    //        hubClientsMock.Verify(x => x.Group($"company-{companyId}"), Times.Exactly(5));

    //        // Verify the correct messages were sent
    //        clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask1.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        clientProxyMock.Verify(
    //            x => x.SendCoreAsync(
    //                "JobTaskUpdated",
    //                It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == jobTask2.Id),
    //                _cancellationToken),
    //            Times.Once);

    //        foreach (var job in jobTask1.Jobs)
    //        {
    //            clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }

    //        foreach (var job in jobTask2.Jobs)
    //        {
    //            clientProxyMock.Verify(
    //                x => x.SendCoreAsync(
    //                    "ScheduledJobUpdated",
    //                    It.Is<object[]>(o => o.Length == 1 && (Guid)o[0] == job.Id),
    //                    _cancellationToken),
    //                Times.Once);
    //        }
    //    }
    }
}
