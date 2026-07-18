using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class ScheduledJobCompanyHelper(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);
        private ScheduledJobRepository CreateRepository()
            => new ScheduledJobRepository(Fixture.Context);

        [Theory]
        [InlineData(5)]
        [InlineData(31)]
        public async Task GetScheduledJobAndExecutionsAsync_ReturnsExecutions(int count)
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            var job = await _schedulerHelper.CreateScheduledJob(task: task);
            var executions = await _schedulerHelper.CreateJobExecutions(task.Id, [job]);

            // duplicate executions until count reached
            while (executions.Count < count)
            {
                var execution = await _schedulerHelper.CreateJobExecution(task.Id, job.Id);
                executions.Add(execution);
            }

            var result = await repo.GetScheduledJobAndExecutionsAsync(
                job.Id,
                company.Id,
                CancellationToken.None);

            var expected = executions
                .OrderByDescending(x => x.StartedAt)
                .Take(30)
                .ToList();

            Assert.NotNull(result);
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected.Select(x => x.Id), result.Select(x => x.Id));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(1)]
        [InlineData(0)]
        public async Task AddScheduledJobAsync_AssignsNextExecutionOrder(int total)
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);

            await _schedulerHelper.CreateScheduledJobs(jobTask: task, total: total);

            var newJob = await _schedulerHelper.CreateScheduledJob(task: task, saveChanges: false);

            await repo.AddScheduledJobAsync(newJob, CancellationToken.None);
            await Fixture.Context.SaveChangesAsync();

            Assert.Equal(total, newJob.ExecutionOrder);
        }

        [Fact]
        public async Task DeleteScheduledJobAsync_RemovesJob_And_ReordersRemaining()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            var jobs = await _schedulerHelper.CreateScheduledJobs(task, total: 3);

            var deletedJob = jobs[1];

            await repo.DeleteScheduledJobAsync(deletedJob.Id, CancellationToken.None);
            await Fixture.Context.SaveChangesAsync();
            Fixture.Context.ChangeTracker.Clear();

            var remaining = Fixture.Context.ScheduledJobs
                .OrderBy(x => x.ExecutionOrder)
                .ToList();

            Assert.Equal(2, remaining.Count);
            Assert.DoesNotContain(remaining, x => x.Id == deletedJob.Id);
            Assert.Equal(0, remaining[0].ExecutionOrder);
            Assert.Equal(1, remaining[1].ExecutionOrder);
        }

        [Fact]
        public async Task ReorderScheduledJobAsync_ReordersExecutionOrder()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            var jobs = await _schedulerHelper.CreateScheduledJobs(task, total: 3);

            var reorderedIds = new List<Guid>
            {
                jobs[2].Id,
                jobs[0].Id,
                jobs[1].Id
            };

            await repo.ReorderScheduledJobAsync(
                reorderedIds,
                company.Id,
                CancellationToken.None);

            await Fixture.Context.SaveChangesAsync();
            Fixture.Context.ChangeTracker.Clear();

            var updated = Fixture.Context.ScheduledJobs
                .OrderBy(x => x.ExecutionOrder)
                .ToList();

            Assert.Equal(jobs[2].Id, updated[0].Id);
            Assert.Equal(jobs[0].Id, updated[1].Id);
            Assert.Equal(jobs[1].Id, updated[2].Id);
        }

        [Fact]
        public async Task ReorderScheduledJobAsync_WhenJobMismatch_Throws()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var invalidIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.ReorderScheduledJobAsync(
                    invalidIds,
                    company.Id,
                    CancellationToken.None));
        }

        [Fact]
        public async Task CleanScheduledJobs_ResetsAllJobResults()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            var jobs = await _schedulerHelper.CreateScheduledJobs(task);

            jobs.ForEach(j => j.Result = JobExecutionStatus.Failed);
            await Fixture.Context.SaveChangesAsync();

            await repo.CleanScheduledJobs(task.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updated = Fixture.Context.ScheduledJobs.ToList();

            Assert.All(updated, j => Assert.Equal(JobExecutionStatus.Pending, j.Result));
        }

        [Fact]
        public async Task CleanScheduledJob_ResetsJob_And_TaskRetries()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id, maxRetries: 5);
            task.Retries = 4;

            var job = await _schedulerHelper.CreateScheduledJob(task: task);
            job.Result = JobExecutionStatus.Failed;
            await Fixture.Context.SaveChangesAsync();

            await repo.CleanScheduledJob(job.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updatedTask = await Fixture.Context.JobTasks.FindAsync(task.Id);
            var updatedJob = await Fixture.Context.ScheduledJobs.FindAsync(job.Id);

            Assert.Equal(0, updatedTask!.Retries);
            Assert.Equal(JobExecutionStatus.Pending, updatedJob!.Result);
        }
    }
}