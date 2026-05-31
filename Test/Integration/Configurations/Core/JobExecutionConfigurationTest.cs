using Domain.Entities.Core;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class JobExecutionConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new(fixture);
    private readonly SchedulerHelper _schedulerHelper = new(fixture);

    // -------------------------------------------------------
    // Foreign key constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenScheduledJobDoesNotExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var (task, _, taskExecution) =
            await _schedulerHelper.CreateTaskAndExecution(saveChanges: false);

        task.CompanyId = company.Id;
        await Fixture.Context.SaveChangesAsync();

        var execution = new JobExecution
        {
            ScheduledJobId = Guid.NewGuid(),
            ScheduledJob = null!,
            JobTaskExecutionId = taskExecution.Id,
            JobTaskExecution = taskExecution,
            Status = JobExecutionStatus.Running,
            TriggerType = JobTriggerType.Scheduled,
            StartedAt = DateTime.UtcNow
        };

        Fixture.Context.JobExecutions.Add(execution);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenJobTaskExecutionDoesNotExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);
        var scheduledJob = await _schedulerHelper.CreateScheduledJob(task: jobTask);

        var execution = await _schedulerHelper.CreateJobExecution(
            jobTaskId: jobTask.Id,
            scheduledJobId: scheduledJob.Id,
            saveChanges: false);

        execution.JobTaskExecutionId = Guid.NewGuid();
        execution.JobTaskExecution = null!;

        Fixture.Context.JobExecutions.Add(execution);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteScheduledJob_DeletesAssociatedJobExecutions()
    {
        var company = await _companyHelper.CreateCompany();
        var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);
        var scheduledJob = await _schedulerHelper.CreateScheduledJob(task: jobTask);

        await _schedulerHelper.CreateJobExecution(
            jobTaskId: jobTask.Id,
            scheduledJobId: scheduledJob.Id);

        Fixture.Context.ScheduledJobs.Remove(scheduledJob);

        await Fixture.Context.SaveChangesAsync();

        var remaining = await Fixture.Context.JobExecutions
            .CountAsync(x => x.ScheduledJobId == scheduledJob.Id);

        Assert.Equal(0, remaining);
    }

    // -------------------------------------------------------
    // NoAction delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteJobTaskExecution_ThrowsDueToNoActionConstraint()
    {
        var company = await _companyHelper.CreateCompany();
        var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);
        var scheduledJob = await _schedulerHelper.CreateScheduledJob(task: jobTask);

        await _schedulerHelper.CreateJobExecution(
            jobTaskId: jobTask.Id,
            scheduledJobId: scheduledJob.Id);

        var taskExecution = await Fixture.Context.JobTaskExecutions.FirstAsync();

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.JobTaskExecutions.Remove(taskExecution));
    }

    // -------------------------------------------------------
    // ErrorMessage (nvarchar(MAX))
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenErrorMessageIsLarge_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();
        var jobTask = await _schedulerHelper.CreateJobTask(companyId: company.Id);
        var scheduledJob = await _schedulerHelper.CreateScheduledJob(task: jobTask);

        var execution = await _schedulerHelper.CreateJobExecution(
            jobTaskId: jobTask.Id,
            scheduledJobId: scheduledJob.Id,
            saveChanges: false);

        execution.ErrorMessage = new string('A', 20000);

        Fixture.Context.JobExecutions.Add(execution);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobExecutions.AnyAsync(x => x.Id == execution.Id);
        Assert.True(exists);
    }
}