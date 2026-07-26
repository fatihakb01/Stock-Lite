using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Infrastructure.Services.Redis;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.Redis
{
    public class SchedulerCacheServiceTest
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly SchedulerCacheService _schedulerCacheService;
        private readonly CancellationToken _cancellationToken;

        public SchedulerCacheServiceTest()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _schedulerCacheService = new SchedulerCacheService(_cacheServiceMock.Object);
            _cancellationToken = CancellationToken.None;
        }

        [Fact]
        public async Task GetAsync_WithValidKey_ReturnsValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = "test-value";

            _cacheServiceMock
                .Setup(x => x.GetAsync<string>(key, _cancellationToken))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _schedulerCacheService.GetAsync<string>(key, _cancellationToken);

            // Assert
            Assert.Equal(expectedValue, result);
            _cacheServiceMock.Verify(x => x.GetAsync<string>(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithNonExistentKey_ReturnsDefault()
        {
            // Arrange
            var key = "non-existent-key";

            _cacheServiceMock
                .Setup(x => x.GetAsync<string>(key, _cancellationToken))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _schedulerCacheService.GetAsync<string>(key, _cancellationToken);

            // Assert
            Assert.Null(result);
            _cacheServiceMock.Verify(x => x.GetAsync<string>(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithComplexType_ReturnsDeserializedValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = new TestClass { Id = 1, Name = "Test" };

            _cacheServiceMock
                .Setup(x => x.GetAsync<TestClass>(key, _cancellationToken))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _schedulerCacheService.GetAsync<TestClass>(key, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue.Id, result.Id);
            Assert.Equal(expectedValue.Name, result.Name);
            _cacheServiceMock.Verify(x => x.GetAsync<TestClass>(key, _cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(30)]
        public async Task SetAsync_WithValueAndExpiry_StoresValue(int? expiryInMinutes)
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            TimeSpan? expiry = expiryInMinutes.HasValue ? 
                TimeSpan.FromMinutes(expiryInMinutes.Value) : 
                null;

            // Act
            await _schedulerCacheService.SetAsync(key, value, expiry, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(
                x => x.SetAsync(key, value, expiry, _cancellationToken),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(30)]
        public async Task SetAsync_WithComplexType_StoresValue(int? expiryInMinutes)
        {
            // Arrange
            var key = "test-key";
            var value = new TestClass { Id = 1, Name = "Test" };
            TimeSpan? expiry = expiryInMinutes.HasValue ? 
                TimeSpan.FromMinutes(expiryInMinutes.Value) : 
                null;

            // Act
            await _schedulerCacheService.SetAsync(key, value, expiry, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(
                x => x.SetAsync(key, value, expiry, _cancellationToken),
                Times.Once);
        }

        [Theory]
        [InlineData(SchedulerKey.JobTask, "jobtask:")]
        [InlineData(SchedulerKey.JobTasks, "jobtasks:")]
        [InlineData(SchedulerKey.JobTaskHistory, "jobtask:history:")]
        [InlineData(SchedulerKey.JobTaskRecentHistory, "jobtasks:recent:history:")]
        [InlineData(SchedulerKey.ScheduledJob, "scheduledjob:")]
        [InlineData(SchedulerKey.ScheduledJobHistory, "scheduledjob:history:")]
        [InlineData(SchedulerKey.SchedulerStatus, "productkpis-schedulerstatus:")]
        public void GetKey_WithValidSchedulerKey_ReturnsCorrectKeyFormat(SchedulerKey schedulerKey, string expectedPrefix)
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedKey = $"{expectedPrefix}{id}";

            // Act
            var result = _schedulerCacheService.GetKey(id, schedulerKey);

            // Assert
            Assert.Equal(expectedKey, result);
        }

        [Fact]
        public void GetKey_WithUnsupportedSchedulerKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var invalidKey = (SchedulerKey)999;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _schedulerCacheService.GetKey(id, invalidKey));
        }

        [Fact]
        public async Task RemoveSchedulerKeys_WithMultipleJobTasks_RemovesAllKeys()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask1 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
            var jobTask2 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
            jobTask1.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask1, total: 1);
            jobTask2.Jobs = SchedulerHelper.CreateScheduledJobsStatic(jobTask2, total: 2);
            var jobTasks = new List<JobTask> { jobTask1, jobTask2 };

            // Act
            await _schedulerCacheService.RemoveSchedulerKeys(jobTasks, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{companyId}", _cancellationToken), Times.Exactly(2));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask1.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask2.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask1.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask2.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:recent:history:{companyId}", _cancellationToken), Times.Exactly(2));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId}", _cancellationToken), Times.Exactly(2));

            foreach (var job in jobTask1.Jobs)
            {
                _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job.Id}", _cancellationToken), Times.Once);
                _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{job.Id}", _cancellationToken), Times.Once);
            }

            foreach (var job in jobTask2.Jobs)
            {
                _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job.Id}", _cancellationToken), Times.Once);
                _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{job.Id}", _cancellationToken), Times.Once);
            }
        }

        [Fact]
        public async Task RemoveSchedulerKeys_WithEmptyJobTaskList_DoesNotRemoveAnyKeys()
        {
            // Arrange
            var jobTasks = new List<JobTask>();

            // Act
            await _schedulerCacheService.RemoveSchedulerKeys(jobTasks, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RemoveSchedulerKeys_WithJobTaskWithNoJobs_RemovesOnlyJobTaskKeys()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
            var jobTasks = new List<JobTask> { jobTask };

            // Act
            await _schedulerCacheService.RemoveSchedulerKeys(jobTasks, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{companyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:recent:history:{companyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId}", _cancellationToken), Times.Once);

            // Verify no scheduled job keys were removed
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(s => s.StartsWith("scheduledjob:")), _cancellationToken), Times.Never);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public async Task RemoveScheduledJobKeysAsync_JobAndHistory_RemovesAllKeys(bool removeHistory, int historyCallCount)
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
            var job = SchedulerHelper.CreateScheduledJobStatic(task: jobTask);
            jobTask.Jobs.Add(job);

            // Act
            await _schedulerCacheService.RemoveScheduledJobKeysAsync(job, _cancellationToken, removeHistory: removeHistory);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{job.Id}", _cancellationToken), Times.Exactly(historyCallCount));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{jobTask.CompanyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask.Id}", _cancellationToken), Times.Exactly(historyCallCount));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:recent:history:{jobTask.CompanyId}", _cancellationToken), Times.Exactly(historyCallCount));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{jobTask.CompanyId}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveScheduledJobKeysAsync_WithJobWithoutJobTask_RemovesOnlyScheduledJobKeys()
        {
            // Arrange
            var job = SchedulerHelper.CreateScheduledJobStatic();
            job.JobTask = null!;

            // Act
            await _schedulerCacheService.RemoveScheduledJobKeysAsync(job, _cancellationToken, removeHistory: true);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{job.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(s => s.StartsWith("jobtask:")), _cancellationToken), Times.Never);
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(s => s.StartsWith("productkpis-schedulerstatus:")), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RemoveScheduledJobKeyAsync_RemovesScheduledJobKey()
        {
            // Arrange
            var job = SchedulerHelper.CreateScheduledJobStatic();

            // Act
            await _schedulerCacheService.RemoveScheduledJobKeyAsync(job, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job.Id}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveScheduledJobKeyAsync_WithDifferentJobs_RemovesCorrectKeys()
        {
            // Arrange
            var job1 = SchedulerHelper.CreateScheduledJobStatic();
            var job2 = SchedulerHelper.CreateScheduledJobStatic();

            // Act
            await _schedulerCacheService.RemoveScheduledJobKeyAsync(job1, _cancellationToken);
            await _schedulerCacheService.RemoveScheduledJobKeyAsync(job2, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job1.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{job2.Id}", _cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public async Task RemoveJobTaskKeysAsync_JobTaskAndHistory_RemovesAllKeys(bool removeHistory, int historyCallCount)
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);

            // Act
            await _schedulerCacheService.RemoveJobTaskKeysAsync(jobTask, _cancellationToken, removeHistory: removeHistory);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{jobTask.CompanyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask.Id}", _cancellationToken), Times.Exactly(historyCallCount));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:recent:history:{jobTask.CompanyId}", _cancellationToken), Times.Exactly(historyCallCount));
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{jobTask.CompanyId}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveJobTaskKeysAsync_WithNullJobTask_DoesNotRemoveAnyKeys()
        {
            // Arrange
            JobTask? jobTask = null;

            // Act
            await _schedulerCacheService.RemoveJobTaskKeysAsync(jobTask, _cancellationToken, removeHistory: true);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RemoveJobTaskKeysAsync_WithJobTask_RemovesSchedulerStatusKey()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);

            // Act
            await _schedulerCacheService.RemoveJobTaskKeysAsync(jobTask, _cancellationToken, removeHistory: true);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveSchedulerStatusKeyAsync_RemovesStatusKey()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            await _schedulerCacheService.RemoveSchedulerStatusKeyAsync(companyId, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveSchedulerStatusKeyAsync_WithDifferentCompanyIds_RemovesCorrectKeys()
        {
            // Arrange
            var companyId1 = Guid.NewGuid();
            var companyId2 = Guid.NewGuid();

            // Act
            await _schedulerCacheService.RemoveSchedulerStatusKeyAsync(companyId1, _cancellationToken);
            await _schedulerCacheService.RemoveSchedulerStatusKeyAsync(companyId2, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId1}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId2}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveJobTaskKeyAsync_WithJobTask_RemovesJobTaskKey()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);

            // Act
            await _schedulerCacheService.RemoveJobTaskKeyAsync(jobTask, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask.Id}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveJobTaskKeyAsync_WithNullJobTask_DoesNotRemoveAnyKey()
        {
            // Arrange
            JobTask? jobTask = null;

            // Act
            await _schedulerCacheService.RemoveJobTaskKeyAsync(jobTask, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RemoveJobTasksKeyAsync_WithJobTask_RemovesJobTasksKey()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);

            // Act
            await _schedulerCacheService.RemoveJobTasksKeyAsync(jobTask, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{jobTask.CompanyId}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveJobTasksKeyAsync_WithNullJobTask_DoesNotRemoveAnyKey()
        {
            // Arrange
            JobTask? jobTask = null;

            // Act
            await _schedulerCacheService.RemoveJobTasksKeyAsync(jobTask, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RemoveJobTasksKeyAsync_WithDifferentJobTasks_RemovesCorrectKeys()
        {
            // Arrange
            var companyId1 = Guid.NewGuid();
            var companyId2 = Guid.NewGuid();
            var jobTask1 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId1);
            var jobTask2 = SchedulerHelper.CreateJobTaskStatic(companyId: companyId2);

            // Act
            await _schedulerCacheService.RemoveJobTasksKeyAsync(jobTask1, _cancellationToken);
            await _schedulerCacheService.RemoveJobTasksKeyAsync(jobTask2, _cancellationToken);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{companyId1}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{companyId2}", _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RemoveScheduledJobKeysAsync_WithJobTaskWithMultipleJobs_RemovesAllKeys()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var jobTask = SchedulerHelper.CreateJobTaskStatic(companyId: companyId);
            var job = SchedulerHelper.CreateScheduledJobsStatic(jobTask, total: 3).First();
            jobTask.Jobs.Add(job);

            // Act
            await _schedulerCacheService.RemoveScheduledJobKeysAsync(job, _cancellationToken, removeHistory: true);

            // Assert
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:{companyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtask:history:{jobTask.Id}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"jobtasks:recent:history:{companyId}", _cancellationToken), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"productkpis-schedulerstatus:{companyId}", _cancellationToken), Times.Once);

            // Verify only the specific scheduled job keys are removed
            foreach (var scheduledJob in jobTask.Jobs)
            {
                if (scheduledJob == job)
                {
                    _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{scheduledJob.Id}", _cancellationToken), Times.Once);
                    _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{scheduledJob.Id}", _cancellationToken), Times.Once);
                }
                else
                {
                    _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:{scheduledJob.Id}", _cancellationToken), Times.Never);
                    _cacheServiceMock.Verify(x => x.RemoveAsync($"scheduledjob:history:{scheduledJob.Id}", _cancellationToken), Times.Never);
                }
            }
        }
    }
}