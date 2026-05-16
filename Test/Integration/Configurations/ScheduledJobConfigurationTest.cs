using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations;

public class ScheduledJobConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask);

        scheduledJob.Name = null!;

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            name: new string('A', 51));

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            name: new string('A', 50));

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.ScheduledJobs.AnyAsync(x =>
            x.Id == scheduledJob.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask);

        scheduledJob.Description = new string('A', 201);

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask);

        scheduledJob.Description = new string('A', 200);

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.ScheduledJobs.AnyAsync(x =>
            x.Id == scheduledJob.Id);

        Assert.True(exists);
    }

    // -------------------------------------------------------
    // Unique constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenExecutionOrderAlreadyExistsForJobTask_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        await _schedulerHelper.CreateScheduledJob(
            task: jobTask,
            executionOrder: 1);

        var duplicate = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            executionOrder: 1);

        Fixture.Context.ScheduledJobs.Add(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenExecutionOrderExistsForDifferentJobTask_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask1 = await _schedulerHelper.CreateJobTask(
            companyId: company.Id,
            jobTaskKey: "TASK-1");

        var jobTask2 = await _schedulerHelper.CreateJobTask(
            companyId: company.Id,
            jobTaskKey: "TASK-2");

        await _schedulerHelper.CreateScheduledJob(
            task: jobTask1,
            executionOrder: 1);

        var second = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask2,
            executionOrder: 1);

        Fixture.Context.ScheduledJobs.Add(second);

        await Fixture.Context.SaveChangesAsync();

        var count = await Fixture.Context.ScheduledJobs.CountAsync(x =>
            x.ExecutionOrder == 1);

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Foreign key constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenJobTaskDoesNotExist_ThrowsException()
    {
        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic();

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Check constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenCustomCalculationWithoutTarget_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.CustomCalculation);

        scheduledJob.Target = null;

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenNonCustomCalculationWithTarget_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.StandardCalculation);

        scheduledJob.Target = "dbo.custom_my_proc";

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenImportJobWithoutImportDataType_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.Import);

        scheduledJob.ImportDataType = null;

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenImportJobWithoutSource_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.Import);

        scheduledJob.Source = null;

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenStandardCalculationWithoutCalculationType_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.StandardCalculation);

        scheduledJob.CalculationType = null;

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenCustomCalculationWithTarget_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.CustomCalculation);

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.ScheduledJobs.AnyAsync(x =>
            x.Id == scheduledJob.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenImportJobWithImportDataTypeAndSource_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.Import);

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.ScheduledJobs.AnyAsync(x =>
            x.Id == scheduledJob.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenStandardCalculationWithCalculationType_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = SchedulerHelper.CreateScheduledJobStatic(
            task: jobTask,
            jobType: JobType.StandardCalculation);

        Fixture.Context.ScheduledJobs.Add(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.ScheduledJobs.AnyAsync(x =>
            x.Id == scheduledJob.Id);

        Assert.True(exists);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteJobTask_DeletesAssociatedScheduledJobs()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        await _schedulerHelper.CreateScheduledJobs(
            jobTask,
            total: 3);

        Fixture.Context.JobTasks.Remove(jobTask);

        await Fixture.Context.SaveChangesAsync();

        var remaining = await Fixture.Context.ScheduledJobs
            .CountAsync(x => x.JobTaskId == jobTask.Id);

        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task DeleteScheduledJob_DeletesAssociatedJobExecutions()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = await _schedulerHelper.CreateScheduledJob(
            task: jobTask);

        await _schedulerHelper.CreateJobExecution(
            jobTask.Id,
            scheduledJob.Id);

        Fixture.Context.ScheduledJobs.Remove(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var remainingExecutions = await Fixture.Context.JobExecutions
            .CountAsync(x => x.ScheduledJobId == scheduledJob.Id);

        Assert.Equal(0, remainingExecutions);
    }
}