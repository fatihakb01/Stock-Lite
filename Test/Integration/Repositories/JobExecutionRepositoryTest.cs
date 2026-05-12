using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class JobExecutionRepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private JobExecutionRepository CreateRepository()
            => new JobExecutionRepository(Fixture.Context);

        [Fact]
        public async Task GetRecentExecutionAsync_WhenExecutionsExist_ReturnsExecutions()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = SchedulerHelper.CreateJobTask(
                name: "Test Task",
                jobTaskKey: "TEST-1",
                companyId: company.Id);

            var jobs = SchedulerHelper.CreateScheduledJobs(task, total: 5);
            var executions = SchedulerHelper.CreateJobExecutions(task.Id, jobs);

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetRecentExecutionAsync(
                company.Id,
                days: 30,
                CancellationToken.None);

            var expected = executions
                .OrderByDescending(x => x.StartedAt)
                .Select(x => x.Id)
                .ToList();

            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
            Assert.Equal(expected, result.Select(x => x.Id).ToList());
        }

        [Fact]
        public async Task GetRecentExecutionAsync_WhenNoExecutionsExist_ReturnsEmptyList()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var result = await repo.GetRecentExecutionAsync(
                company.Id,
                days: 30,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentExecutionAsync_ReturnsOnlyExecutionsForCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany("Company 1");
            var company2 = await _companyHelper.CreateCompany("Company 2");

            // Company 1
            var task1 = SchedulerHelper.CreateJobTask(
                name: "Task 1",
                jobTaskKey: "TASK-1",
                companyId: company1.Id);

            var jobs1 = SchedulerHelper.CreateScheduledJobs(task1, total: 3);
            var executions1 = SchedulerHelper.CreateJobExecutions(task1.Id, jobs1);

            // Company 2
            var task2 = SchedulerHelper.CreateJobTask(
                name: "Task 2",
                jobTaskKey: "TASK-2",
                companyId: company2.Id);

            var jobs2 = SchedulerHelper.CreateScheduledJobs(task2, total: 2);
            var executions2 = SchedulerHelper.CreateJobExecutions(task2.Id, jobs2);

            await Fixture.Context.JobTasks.AddRangeAsync(task1, task2);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs1);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs2);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions1);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions2);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetRecentExecutionAsync(
                company1.Id,
                days: 30,
                CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.All(result, x => Assert.Equal(company1.Id, x.ScheduledJob.JobTask.CompanyId));
        }

        [Fact]
        public async Task GetRecentExecutionAsync_IgnoresOldExecutions()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = SchedulerHelper.CreateJobTask(
                name: "Test Task",
                jobTaskKey: "TEST-1",
                companyId: company.Id);

            var jobs = SchedulerHelper.CreateScheduledJobs(task, total: 2);
            var executions = SchedulerHelper.CreateJobExecutions(task.Id, jobs);

            // Make one execution old
            executions[0].StartedAt = DateTime.UtcNow.AddDays(-40);

            // Keep one recent
            executions[1].StartedAt = DateTime.UtcNow.AddDays(-5);

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetRecentExecutionAsync(
                company.Id,
                days: 30,
                CancellationToken.None);

            Assert.Single(result);
            Assert.True(result[0].StartedAt >= DateTime.UtcNow.AddDays(-30));
        }

        [Fact]
        public async Task GetRecentExecutionAsync_ReturnsOrderedByStartedAtDescending()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = SchedulerHelper.CreateJobTask(
                name: "Test Task",
                jobTaskKey: "TEST-1",
                companyId: company.Id);

            var jobs = SchedulerHelper.CreateScheduledJobs(task, total: 3);
            var executions = SchedulerHelper.CreateJobExecutions(task.Id, jobs);

            executions[0].StartedAt = DateTime.UtcNow.AddMinutes(-30);
            executions[1].StartedAt = DateTime.UtcNow.AddMinutes(-10);
            executions[2].StartedAt = DateTime.UtcNow.AddMinutes(-20);

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.ScheduledJobs.AddRangeAsync(jobs);
            await Fixture.Context.JobExecutions.AddRangeAsync(executions);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetRecentExecutionAsync(
                company.Id,
                days: 30,
                CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.True(result[0].StartedAt >= result[1].StartedAt);
            Assert.True(result[1].StartedAt >= result[2].StartedAt);
        }
    }
}