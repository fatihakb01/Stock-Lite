using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class JobTaskRepositoryTest(DatabaseFixture fixture) 
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private JobTaskRepository CreateRepository()
            => new JobTaskRepository(Fixture.Context);

        [Theory]
        [InlineData(5)]
        [InlineData(31)]
        public async Task GetJobsAndExecutionsAsync_ReturnsJobsAndExecutions(int count)
        {
            var repo = CreateRepository();

            var now = DateTime.UtcNow;
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id,
                scheduledAt: now);
            var jobs = SchedulerHelper.CreateScheduledJobs(jobTask: task, total: count);
            var executions = SchedulerHelper.CreateJobExecutions(task.Id, jobs);

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetJobsAndExecutionsAsync(task.Id, company.Id, CancellationToken.None);
            var expected = executions.OrderByDescending(x => x.StartedAt).Take(30).ToList();

            Assert.NotNull(result);
            Assert.Equal(expected.Select(x => x.Id), result.Select(x => x.Id));
            Assert.Equal(expected.Count, result.Count);
        }

        [Fact]
        public async Task GetNextDueTaskAsync_ReturnsNextDueTask()
        {
            var repo = CreateRepository();

            var now = DateTime.UtcNow;
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            // due
            var task1 = SchedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id, 
                scheduledAt: now.AddMinutes(-5));

            // future
            var task2 = SchedulerHelper.CreateJobTask(
                name: "Test Task 2",
                jobTaskKey: "TEST-2",
                companyId: company.Id, 
                scheduledAt: now.AddMinutes(5));

            await Fixture.Context.JobTasks.AddRangeAsync(task1, task2);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetNextDueTaskAsync(company.Id, now, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(task1.Id, result!.Id);
        }

        [Fact]
        public async Task GetNextDueTaskAsync_IgnoresArchived()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
            task.IsArchived = true;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetNextDueTaskAsync(company.Id, DateTime.UtcNow, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetNextDueTaskAsync_IgnoresManualOnly()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
            task.AllowManualTriggerOnly = true;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetNextDueTaskAsync(company.Id, DateTime.UtcNow, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetNextDueTaskAsync_RespectsRetryLimit()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
            task.Retries = 3;
            task.MaxRetries = 3;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetNextDueTaskAsync(company.Id, DateTime.UtcNow, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task HasExhaustedTaskAsync_WhenExists_ReturnsTrue()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;
            task.Retries = 3;
            task.MaxRetries = 3;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.HasExhaustedTaskAsync(company.Id, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task HasExhaustedTaskAsync_WhenNone_ReturnsFalse()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.HasExhaustedTaskAsync(company.Id, CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task TryLockTaskAsync_WhenUnlocked_UpdatesRow()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Pending;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.TryLockTaskAsync(task.Id, DateTime.UtcNow);

            Assert.True(result);

            Fixture.Context.ChangeTracker.Clear();

            var updated = await Fixture.Context.JobTasks.FindAsync(task.Id);
            Assert.Equal(JobExecutionStatus.Running, updated!.Result);
        }

        [Fact]
        public async Task TryLockTaskAsync_WhenAlreadyRunning_ReturnsFalse()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Running;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.TryLockTaskAsync(task.Id, DateTime.UtcNow);

            Assert.False(result);
        }

        [Fact]
        public async Task CleanJobTask_ResetsFields()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;
            task.Retries = 5;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            await repo.CleanJobTask(task.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updated = await Fixture.Context.JobTasks.FindAsync(task.Id);

            Assert.Equal(0, updated!.Retries);
            Assert.Equal(JobExecutionStatus.Pending, updated.Result);
        }

        [Fact]
        public async Task ResetScheduler_WhenAllSucceeded_ResetsTasks_And_ScheduledJobs()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            // Create succeeded JobTasks
            var tasks = SchedulerHelper.CreateJobTasks();

            tasks.ForEach(t =>
            {
                t.CompanyId = company.Id;
                t.Result = JobExecutionStatus.Succeeded;
                t.IsArchived = false;
            });

            await Fixture.Context.JobTasks.AddRangeAsync(tasks);

            // Create succeeded ScheduledJobs linked to tasks
            var scheduledJobs = new List<ScheduledJob>();

            foreach (var task in tasks)
            {
                var jobs = SchedulerHelper.CreateScheduledJobs(task);

                jobs.ForEach(j =>
                {
                    j.Result = JobExecutionStatus.Succeeded;
                    j.IsArchived = false;
                });

                scheduledJobs.AddRange(jobs);
            }

            await Fixture.Context.ScheduledJobs.AddRangeAsync(scheduledJobs);
            await Fixture.Context.SaveChangesAsync();

            await repo.ResetScheduler(company.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updatedTasks = Fixture.Context.JobTasks.ToList();
            var updatedJobs = Fixture.Context.ScheduledJobs.ToList();

            Assert.All(updatedTasks, t => Assert.Equal(JobExecutionStatus.Pending, t.Result));
            Assert.All(updatedJobs, j => Assert.Equal(JobExecutionStatus.Pending, j.Result));
        }

        [Fact]
        public async Task ResetScheduler_WhenNotAllSucceeded_DoesNotResetTasks_Or_ScheduledJobs()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            // One succeeded task
            var successTask = SchedulerHelper.CreateJobTask(
                name: "Success Task",
                jobTaskKey: "SUCCESS-TASK",
                companyId: company.Id);

            successTask.Result = JobExecutionStatus.Succeeded;

            // One failed task
            var failedTask = SchedulerHelper.CreateJobTask(
                name: "Failed Task",
                jobTaskKey: "FAILED-TASK",
                companyId: company.Id);

            failedTask.Result = JobExecutionStatus.Failed;

            await Fixture.Context.JobTasks.AddRangeAsync(successTask, failedTask);

            // Scheduled jobs linked to tasks
            var successJobs = SchedulerHelper.CreateScheduledJobs(successTask);
            var failedJobs = SchedulerHelper.CreateScheduledJobs(failedTask);
            var scheduledJobs = successJobs
                .Concat(failedJobs)
                .ToList();

            scheduledJobs.ForEach(j =>
            {
                j.Result = JobExecutionStatus.Succeeded;
                j.IsArchived = false;
            });

            await Fixture.Context.ScheduledJobs.AddRangeAsync(scheduledJobs);
            await Fixture.Context.SaveChangesAsync();

            await repo.ResetScheduler(company.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updatedTasks = Fixture.Context.JobTasks.ToList();
            var updatedJobs = Fixture.Context.ScheduledJobs.ToList();

            Assert.Contains(updatedTasks, t => t.Result == JobExecutionStatus.Failed);
            Assert.Contains(updatedTasks, t => t.Result == JobExecutionStatus.Succeeded);
            Assert.All(updatedJobs, j => Assert.Equal(JobExecutionStatus.Succeeded, j.Result));
        }

        [Fact]
        public async Task ResetScheduler_IgnoresArchivedScheduledJobs()
        {
            var repo = CreateRepository();
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            // Create succeeded JobTask
            var task = SchedulerHelper.CreateJobTask(
                name: "Test Task",
                jobTaskKey: "TEST-TASK",
                companyId: company.Id);

            task.Result = JobExecutionStatus.Succeeded;
            task.IsArchived = false;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            // Create active ScheduledJob
            var activeJob = SchedulerHelper.CreateScheduledJob(
                executionOrder: 0,
                name: "Active Job",
                task: task);
            activeJob.Result = JobExecutionStatus.Succeeded;
            activeJob.IsArchived = false;

            // Create archived ScheduledJob
            var archivedJob = SchedulerHelper.CreateScheduledJob(
                executionOrder: 1,
                name: "Archived Job",
                task: task);
            archivedJob.Result = JobExecutionStatus.Succeeded;
            archivedJob.IsArchived = true;

            await Fixture.Context.ScheduledJobs.AddRangeAsync(activeJob, archivedJob);
            await Fixture.Context.SaveChangesAsync();

            await repo.ResetScheduler(company.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updatedTask = await Fixture.Context.JobTasks.FindAsync(task.Id);
            var updatedActiveJob = await Fixture.Context.ScheduledJobs.FindAsync(activeJob.Id);
            var updatedArchivedJob = await Fixture.Context.ScheduledJobs.FindAsync(archivedJob.Id);

            Assert.Equal(JobExecutionStatus.Pending, updatedTask!.Result);
            Assert.Equal(JobExecutionStatus.Pending, updatedActiveJob!.Result);
            Assert.Equal(JobExecutionStatus.Succeeded, updatedArchivedJob!.Result);
        }
    }
}
