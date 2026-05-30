using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class ProductKpiRepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private readonly ProductHelper _productHelper = new ProductHelper(fixture);
        private readonly SchedulerHelper _schedulerHelper = new SchedulerHelper(fixture);
        private readonly ProductKpiHelper _productKpiHelper = new ProductKpiHelper(fixture);

        private ProductKpiRepository CreateRepository()
            => new ProductKpiRepository(Fixture.Context);

        [Fact]
        public async Task GetProductKpis_WhenExists_ReturnsProductKpi()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            await _productKpiHelper.CreateProductKpi(
                company: company,
                totalProductCount: 10,
                belowSafetyStockCount: 5,
                recommendedOrdersCount: 15,
                withoutStockAtLeadTimeCount: 2,
                withoutForecastCount: 9);

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
            var company = await _companyHelper.CreateCompany();

            var failedTask = await _schedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id);
            failedTask.Result = JobExecutionStatus.Failed;

            var runningTask = await _schedulerHelper.CreateJobTask(
                name: "Test Task 2",
                jobTaskKey: "TEST-2",
                companyId: company.Id);
            runningTask.Result = JobExecutionStatus.Running;

            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetSchedulerStatus(company.Id, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Failed, result);
        }

        [Fact]
        public async Task GetSchedulerStatus_WhenRunningTaskExists_ReturnsRunning()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var runningTask = await _schedulerHelper.CreateJobTask(
                name: "Test Task 1",
                jobTaskKey: "TEST-1",
                companyId: company.Id);
            runningTask.Result = JobExecutionStatus.Running;

            var succeededTask = await _schedulerHelper.CreateJobTask(
                name: "Test Task 2",
                jobTaskKey: "TEST-2",
                companyId: company.Id);
            succeededTask.Result = JobExecutionStatus.Succeeded;

            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetSchedulerStatus(company.Id, CancellationToken.None);

            Assert.Equal(JobExecutionStatus.Running, result);
        }

        [Fact]
        public async Task UpdateSchedulerStatus_WhenProductKpiMissing_CreatesProductKpi()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Running;

            await Fixture.Context.SaveChangesAsync();

            await repo.UpdateSchedulerStatus(company.Id, CancellationToken.None);
            var result = Fixture.Context.ProductKpis.Single(x => x.CompanyId == company.Id);

            Assert.Equal(JobExecutionStatus.Running, result.SchedulerStatus);
        }

        [Fact]
        public async Task UpdateSchedulerStatus_WhenChanged_UpdatesStatus()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            await _productKpiHelper.CreateProductKpi(
                company: company,
                status: JobExecutionStatus.Pending,
                saveChanges: false);

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;

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
            var company = await _companyHelper.CreateCompany();

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Succeeded;
            await _productHelper.CreateProducts(company);

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
            var company = await _companyHelper.CreateCompany();

            await _productKpiHelper.CreateProductKpi(
                company: company,
                totalProductCount: 0,
                belowSafetyStockCount: 0,
                recommendedOrdersCount: 0,
                withoutStockAtLeadTimeCount: 0,
                withoutForecastCount: 0,
                status: JobExecutionStatus.Pending);

            var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);
            task.Result = JobExecutionStatus.Failed;

            var products = await _productHelper.CreateProducts(company);

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