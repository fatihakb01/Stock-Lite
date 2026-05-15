using Domain.Enums;
using Infrastructure.Services.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Scheduler
{
    public class JobDispatcherTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);

        // JobDispatcher needs a real IServiceProvider for CreateScope()
        // We build a minimal one that just provides DataContext
        private JobDispatcher CreateDispatcher()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Fixture.Context);

            var provider = services.BuildServiceProvider();
            return new JobDispatcher(provider, Fixture.Context);
        }

        // -------------------------------------------------------
        // HandleCustomCalculation — procedure existence check
        // -------------------------------------------------------
        [Theory]
        [InlineData("custom_my_proc")]
        [InlineData("dbo.custom_my_proc")]
        public async Task HandleCustomCalculation_WhenProcedureDoesNotExist_ThrowsInvalidOperationException(string target)
        {
            var dispatcher = CreateDispatcher();
            var company = await _companyHelper.CreateCompany();
            var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);

            var job = await _schedulerHelper.CreateScheduledJob(
                jobType: JobType.CustomCalculation, 
                task: jobTask,
                saveChanges: false);
            job.Target = target;
            await Fixture.Context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.DispatchAsync(job, CancellationToken.None));

            Assert.Contains("does not exist", ex.Message);
            Assert.Contains(target, ex.Message);
        }

        [Theory]
        [InlineData("custom_my_proc")]
        [InlineData("dbo.custom_my_proc")]
        public async Task HandleCustomCalculation_WhenProcedureExists_ExecutesSuccessfully(string target)
        {
            var dispatcher = CreateDispatcher();
            var company = await _companyHelper.CreateCompany();
            var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);

            // Create a real custom procedure in the test database
            await Fixture.Context.Database.ExecuteSqlRawAsync(@"
                CREATE OR ALTER PROCEDURE dbo.custom_my_proc
                AS
                BEGIN
                    SET NOCOUNT ON;
                    -- intentionally empty
                END");

            try
            {
                var job = await _schedulerHelper.CreateScheduledJob(
                    jobType: JobType.CustomCalculation,
                    task: jobTask,
                    saveChanges: false);
                job.Target = target;
                await Fixture.Context.SaveChangesAsync();

                // Should complete without throwing
                await dispatcher.DispatchAsync(job, CancellationToken.None);
            }
            finally
            {
                // Clean up the test procedure
                await Fixture.Context.Database.ExecuteSqlRawAsync(
                    "DROP PROCEDURE IF EXISTS dbo.custom_my_proc");
            }
        }

        [Theory]
        [InlineData("custom_my_proc")]
        [InlineData("dbo.custom_my_proc")]
        public async Task HandleCustomCalculation_WhenProcedureThrows_PropagatesException(string target)
        {
            var dispatcher = CreateDispatcher();
            var company = await _companyHelper.CreateCompany();
            var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);

            // Create a procedure that deliberately raises an error
            await Fixture.Context.Database.ExecuteSqlRawAsync(@"
                CREATE OR ALTER PROCEDURE dbo.custom_my_proc
                AS
                BEGIN
                    RAISERROR ('Intentional test failure', 16, 1);
                END");

            try
            {
                var job = await _schedulerHelper.CreateScheduledJob(
                    jobType: JobType.CustomCalculation,
                    task: jobTask,
                    saveChanges: false);
                job.Target = target;
                await Fixture.Context.SaveChangesAsync();

                await Assert.ThrowsAnyAsync<Exception>(() =>
                    dispatcher.DispatchAsync(job, CancellationToken.None));
            }
            finally
            {
                await Fixture.Context.Database.ExecuteSqlRawAsync(
                    "DROP PROCEDURE IF EXISTS dbo.custom_my_proc");
            }
        }
    }
}