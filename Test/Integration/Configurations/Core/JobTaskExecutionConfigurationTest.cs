using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class JobTaskExecutionConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);

    // -------------------------------------------------------
    // Foreign key constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenJobTaskDoesNotExist_ThrowsException()
    {
        var execution = SchedulerHelper.CreateJobTaskExecution(
            jobTaskId: Guid.NewGuid(),
            status: JobExecutionStatus.Running,
            triggerType: JobTriggerType.Scheduled);

        Fixture.Context.JobTaskExecutions.Add(execution);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteJobTask_DeletesAssociatedJobTaskExecutions()
    {
        var company = await _companyHelper.CreateCompany();

        var (task, _, execution) =
            await _schedulerHelper.CreateTaskAndExecution(saveChanges: false);

        task.CompanyId = company.Id;

        await Fixture.Context.SaveChangesAsync();

        Fixture.Context.JobTasks.Remove(task);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTaskExecutions
            .AnyAsync(x => x.Id == execution.Id);

        Assert.False(exists);
    }

    // -------------------------------------------------------
    // NoAction delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteJobTaskExecution_WhenJobExecutionsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var (task, job, taskExecution) =
            await _schedulerHelper.CreateTaskAndExecution(saveChanges: false);

        task.CompanyId = company.Id;

        await Fixture.Context.SaveChangesAsync();

        await _schedulerHelper.CreateJobExecution(
            jobTaskId: task.Id,
            scheduledJobId: job.Id,
            jobTaskExecution: taskExecution);

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

        var (task, _, execution) =
            await _schedulerHelper.CreateTaskAndExecution(saveChanges: false);

        task.CompanyId = company.Id;

        execution.ErrorMessage = new string('A', 20000);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTaskExecutions
            .AnyAsync(x => x.Id == execution.Id);

        Assert.True(exists);
    }
}