using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations;

public class JobTaskConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenJobTaskKeyIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id);

        jobTask.JobTaskKey = null!;

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id);

        jobTask.Name = null!;

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenJobTaskKeyExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id,
            jobTaskKey: new string('A', 51));

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenJobTaskKeyIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id,
            jobTaskKey: new string('A', 50));

        Fixture.Context.JobTasks.Add(jobTask);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTasks.AnyAsync(x =>
            x.JobTaskKey == jobTask.JobTaskKey &&
            x.CompanyId == company.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id,
            name: new string('A', 51));

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id,
            name: new string('A', 50));

        Fixture.Context.JobTasks.Add(jobTask);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTasks.AnyAsync(x =>
            x.Name == jobTask.Name &&
            x.CompanyId == company.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id);

        jobTask.Description = new string('A', 201);

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id);

        jobTask.Description = new string('A', 200);

        Fixture.Context.JobTasks.Add(jobTask);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTasks.AnyAsync(x =>
            x.Description == jobTask.Description &&
            x.CompanyId == company.Id);

        Assert.True(exists);
    }

    // -------------------------------------------------------
    // Unique constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenJobTaskKeyAlreadyExistsForCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _schedulerHelper.CreateJobTask(
            companyId: company.Id,
            jobTaskKey: "IMPORT-001");

        var duplicate = SchedulerHelper.CreateJobTaskStatic(
            companyId: company.Id,
            jobTaskKey: "IMPORT-001");

        Fixture.Context.JobTasks.Add(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenJobTaskKeyExistsForDifferentCompany_SavesSuccessfully()
    {
        var company1 = await _companyHelper.CreateCompany("Company 1");
        var company2 = await _companyHelper.CreateCompany("Company 2");

        await _schedulerHelper.CreateJobTask(
            companyId: company1.Id,
            jobTaskKey: "IMPORT-001");

        var second = SchedulerHelper.CreateJobTaskStatic(
            companyId: company2.Id,
            jobTaskKey: "IMPORT-001");

        Fixture.Context.JobTasks.Add(second);

        await Fixture.Context.SaveChangesAsync();

        var count = await Fixture.Context.JobTasks.CountAsync(x =>
            x.JobTaskKey == "IMPORT-001");

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Foreign key constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenCompanyDoesNotExist_ThrowsException()
    {
        var jobTask = SchedulerHelper.CreateJobTaskStatic(
            companyId: Guid.NewGuid());

        Fixture.Context.JobTasks.Add(jobTask);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteCompany_DeletesAssociatedJobTasks()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        Fixture.Context.Companies.Remove(company);

        await Fixture.Context.SaveChangesAsync();

        var exists = await Fixture.Context.JobTasks.AnyAsync(x =>
            x.Id == jobTask.Id);

        Assert.False(exists);
    }

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

        var remainingJobs = await Fixture.Context.ScheduledJobs
            .CountAsync(x => x.JobTaskId == jobTask.Id);

        Assert.Equal(0, remainingJobs);
    }

    // -------------------------------------------------------
    // NoAction delete behavior
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteJobTask_WhenExecutionsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var jobTask = await _schedulerHelper.CreateJobTask(
            companyId: company.Id);

        var scheduledJob = await _schedulerHelper.CreateScheduledJob(
            task: jobTask);

        await _schedulerHelper.CreateJobExecution(
            jobTask.Id,
            scheduledJob.Id);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.JobTasks.Remove(jobTask));

        Fixture.Context.ChangeTracker.Clear();
    }
}