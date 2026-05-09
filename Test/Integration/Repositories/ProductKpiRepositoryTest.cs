using Domain.Entities.Derived;
using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class ProductKpiCompanyHelper(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private ProductKpiRepository CreateRepository()
            => new ProductKpiRepository(Fixture.Context);

        [Fact]
        public async Task GetProductKpis_WhenExists_ReturnsProductKpi()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var kpi = new ProductKpi
            {
                CompanyId = company.Id,
                Company = company,
                TotalProductCount = 10,
                BelowSafetyStockCount = 5,
                RecommendedOrdersCount = 15,
                WithoutStockAtLeadTimeCount = 2,
                WithoutForecastCount = 9
            };

            await Fixture.Context.ProductKpis.AddAsync(kpi);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetProductKpis(company.Id, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(company.Id, result.CompanyId);
            Assert.Equal(10, result.TotalProductCount);
            Assert.Equal(5, result.BelowSafetyStockCount);
            Assert.Equal(15, result.RecommendedOrdersCount);
            Assert.Equal(2, result.WithoutStockAtLeadTimeCount);
            Assert.Equal(9, result.WithoutForecastCount);
        }

        [Fact]
        public async Task GetProductKpis_WhenMissing_Throws()
        {
            var repo = CreateRepository();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.GetProductKpis(Guid.NewGuid(), CancellationToken.None));
        }

        [Fact]
        public async Task GetSchedulerStatus_WhenFailedTaskExists_ReturnsFailed()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var failedTask = SchedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id);
            failedTask.Result = JobExecutionStatus.Failed;

            var runningTask = SchedulerHelper.CreateJobTask(
                name: "Test Task 2",
                jobTaskKey: "TEST-2",
                companyId: company.Id);
            runningTask.Result = JobExecutionStatus.Running;

            await Fixture.Context.JobTasks.AddRangeAsync(failedTask, runningTask);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetSchedulerStatus(company.Id, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
        }

        [Fact]
        public async Task GetSchedulerStatus_WhenRunningTaskExists_ReturnsRunning()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var runningTask = SchedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id);
            runningTask.Result = JobExecutionStatus.Running;

            var succeededTask = SchedulerHelper.CreateJobTask(
                name: "Test Task 2",
                jobTaskKey: "TEST-2",
                companyId: company.Id);
            succeededTask.Result = JobExecutionStatus.Succeeded;

            await Fixture.Context.JobTasks.AddRangeAsync(runningTask, succeededTask);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetSchedulerStatus(company.Id, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Running, result);
        }

        [Fact]
        public async Task UpdateSchedulerStatus_WhenProductKpiMissing_CreatesProductKpi()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Running;

            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            await repo.UpdateSchedulerStatus(company.Id, CancellationToken.None);
            var result = Fixture.Context.ProductKpis.Single(x => x.CompanyId == company.Id);

            Assert.Equal(JobExecutionStatus.Running, result.SchedulerStatus);
        }

        [Fact]
        public async Task UpdateSchedulerStatus_WhenChanged_UpdatesStatus()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var kpi = new ProductKpi
            {
                CompanyId = company.Id,
                Company = company,
                SchedulerStatus = JobExecutionStatus.Pending
            };

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;

            await Fixture.Context.ProductKpis.AddAsync(kpi);
            await Fixture.Context.JobTasks.AddAsync(task);
            await Fixture.Context.SaveChangesAsync();

            await repo.UpdateSchedulerStatus(company.Id, CancellationToken.None);
            Fixture.Context.ChangeTracker.Clear();

            var updated = Fixture.Context.ProductKpis.Single(x => x.CompanyId == company.Id);

            Assert.Equal(JobExecutionStatus.Failed, updated.SchedulerStatus);
        }

        [Fact]
        public async Task UpdateProductKpis_WhenMissing_CreatesProductKpi()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Succeeded;

            await Fixture.Context.JobTasks.AddAsync(task);

            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();
            await repo.UpdateProductKpis(company.Id, CancellationToken.None);

            Fixture.Context.ChangeTracker.Clear();
            var result = Fixture.Context.ProductKpis.Single(x => x.CompanyId == company.Id);

            Assert.Equal(JobExecutionStatus.Succeeded, result.SchedulerStatus);
            Assert.Equal(2, result.TotalProductCount);
            Assert.Equal(1, result.BelowSafetyStockCount);
            Assert.Equal(1, result.RecommendedOrdersCount);
            Assert.Equal(1, result.WithoutStockAtLeadTimeCount);
            Assert.Equal(1, result.WithoutForecastCount);
        }

        [Fact]
        public async Task UpdateProductKpis_WhenValuesChanged_UpdatesExistingKpi()
        {
            var repo = CreateRepository();
            var company = CompanyHelper.CreateCompany();

            await Fixture.Context.Companies.AddAsync(company);
            await Fixture.Context.SaveChangesAsync();

            var existing = new ProductKpi
            {
                CompanyId = company.Id,
                Company = company,
                TotalProductCount = 0,
                BelowSafetyStockCount = 0,
                RecommendedOrdersCount = 0,
                WithoutForecastCount = 0,
                WithoutStockAtLeadTimeCount = 0,
                SchedulerStatus = JobExecutionStatus.Pending
            };

            await Fixture.Context.ProductKpis.AddAsync(existing);

            var task = SchedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;

            await Fixture.Context.JobTasks.AddAsync(task);

            var products = ProductHelper.CreateProducts(company);

            Fixture.Context.Products.AddRange(products);
            await Fixture.Context.SaveChangesAsync();
            await repo.UpdateProductKpis(company.Id, CancellationToken.None);

            Fixture.Context.ChangeTracker.Clear();
            var updated = Fixture.Context.ProductKpis.Single(x => x.CompanyId == company.Id);

            Assert.Equal(JobExecutionStatus.Failed, updated.SchedulerStatus);
            Assert.Equal(2, updated.TotalProductCount);
            Assert.Equal(1, updated.BelowSafetyStockCount);
            Assert.Equal(1, updated.RecommendedOrdersCount);
            Assert.Equal(1, updated.WithoutStockAtLeadTimeCount);
            Assert.Equal(1, updated.WithoutForecastCount);
        }
    }
}