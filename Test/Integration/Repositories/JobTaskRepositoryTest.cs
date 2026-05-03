using Domain.Enums;
using Infrastructure.Repositories;
using Test.Integration.Helper;
using Test.Unit.Services.Scheduler;

namespace Test.Integration.Repositories
{
    public class JobTaskRepositoryTest(DatabaseFixture fixture) 
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private JobTaskRepository CreateRepository()
            => new JobTaskRepository(Fixture.Context);

        [Fact]
        public async Task GetNextDueTaskAsync_ReturnsNextDueTask()
        {
            var repo = CreateRepository();

            var now = DateTime.UtcNow;
            var company = RepositoryTest.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            // due
            var task1 = JobTaskDispatcherTest.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id, 
                scheduledAt: now.AddMinutes(-5));

            // future
            var task2 = JobTaskDispatcherTest.CreateJobTask(
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id, scheduledAt: DateTime.UtcNow.AddMinutes(-1));
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
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

            var task = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
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

        /*
         * ISSUE IS THAT EACH SCHEDULEDJOB HAS ALL SORTS OF CONSTRAINTS, SO YOU NEED TO CREATE A SCHEDLEDJOB IN A SPECIAL WAY
         * SEE JobDispatcherTest.CreateImportJob, JobDispatcherTest.CreateCalculationJob, JobDispatcherTest.CreateCustomJob
         * BUT WE NEED TO CONNECT THE SCHEDULEDJOB TO IT'S CORRESPONDING JOBTASK
        */

        //[Fact]
        //public async Task ResetScheduler_WhenAllSucceeded_ResetsTasks_And_ScheduledJobs()
        //{
        //    var repo = CreateRepository();
        //    var company = RepositoryTest.CreateCompany();

        //    await Fixture.Context.Companies.AddAsync(company);
        //    await Fixture.Context.SaveChangesAsync();

        //    // Create tasks
        //    var tasks = JobTaskDispatcherTest.CreateJobTasks(3);
        //    tasks.ForEach(t =>
        //    {
        //        t.CompanyId = company.Id;
        //        t.Result = JobExecutionStatus.Succeeded;
        //    });

        //    await Fixture.Context.JobTasks.AddRangeAsync(tasks);
        //    await Fixture.Context.SaveChangesAsync();

        //    // Create ScheduledJobs linked to tasks
        //    var scheduledJobs = tasks.Select(t =>
        //        JobTaskDispatcherTest.CreateScheduledJob(t.Id)
        //    ).ToList();

        //    scheduledJobs.ForEach(j =>
        //    {
        //        j.Result = JobExecutionStatus.Succeeded;
        //        j.IsArchived = false;
        //    });

        //    await Fixture.Context.ScheduledJobs.AddRangeAsync(scheduledJobs);
        //    await Fixture.Context.SaveChangesAsync();

        //    // Act
        //    await repo.ResetScheduler(company.Id, CancellationToken.None);
        //    Fixture.Context.ChangeTracker.Clear();

        //    var updatedTasks = Fixture.Context.JobTasks.ToList();
        //    var updatedJobs = Fixture.Context.ScheduledJobs.ToList();

        //    // Assert JobTasks
        //    Assert.All(updatedTasks, t =>
        //        Assert.Equal(JobExecutionStatus.Pending, t.Result));

        //    // Assert ScheduledJobs
        //    Assert.All(updatedJobs, j =>
        //        Assert.Equal(JobExecutionStatus.Pending, j.Result));
        //}

        //    [Fact]
        //    public async Task ResetScheduler_WhenNotAllSucceeded_DoesNotResetTasks_Or_ScheduledJobs()
        //    {
        //        var repo = CreateRepository();
        //        var company = RepositoryTest.CreateCompany();

        //        await Fixture.Context.Companies.AddAsync(company);
        //        await Fixture.Context.SaveChangesAsync();

        //        // One success, one failure
        //        var successTask = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
        //        successTask.Result = JobExecutionStatus.Succeeded;

        //        var failedTask = JobTaskDispatcherTest.CreateJobTask(companyId: company.Id);
        //        failedTask.Result = JobExecutionStatus.Failed;

        //        await Fixture.Context.JobTasks.AddRangeAsync(successTask, failedTask);
        //        await Fixture.Context.SaveChangesAsync();

        //        var scheduledJobs = new List<ScheduledJob>
        //        {
        //            JobTaskDispatcherTest.CreateScheduledJob(successTask.Id),
        //            JobTaskDispatcherTest.CreateScheduledJob(failedTask.Id)
        //        };

        //        scheduledJobs.ForEach(j =>
        //        {
        //            j.Result = JobExecutionStatus.Succeeded;
        //            j.IsArchived = false;
        //        });

        //        await Fixture.Context.ScheduledJobs.AddRangeAsync(scheduledJobs);
        //        await Fixture.Context.SaveChangesAsync();

        //        // Act
        //        await repo.ResetScheduler(company.Id, CancellationToken.None);
        //        Fixture.Context.ChangeTracker.Clear();

        //        var tasks = Fixture.Context.JobTasks.ToList();
        //        var jobs = Fixture.Context.ScheduledJobs.ToList();

        //        // Assert tasks unchanged
        //        Assert.Contains(tasks, t => t.Result == JobExecutionStatus.Failed);
        //        Assert.Contains(tasks, t => t.Result == JobExecutionStatus.Succeeded);

        //        // Assert scheduled jobs unchanged
        //        Assert.All(jobs, j =>
        //            Assert.Equal(JobExecutionStatus.Succeeded, j.Result));
        //    }
    }
}
