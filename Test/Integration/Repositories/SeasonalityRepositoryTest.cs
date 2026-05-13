using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class SeasonalityRepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private readonly ProductHelper _productHelper = new ProductHelper(fixture);
        private readonly SalesAggregateHelper _salesHelper = new SalesAggregateHelper(fixture);
        private readonly SeasonalityHelper _seasonalityHelper = new SeasonalityHelper(fixture);
        private readonly DateTime _now = DateTime.UtcNow.Date;

        private SeasonalityRepository CreateRepository() =>
            new(Fixture.Context);

        [Fact]
        public async Task GetSeasonalityByProductAsync_WhenSeasonalityExists_ReturnsRecords()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _seasonalityHelper.CreateSeasonality(product, 1.0m);

            var result = await repo.GetSeasonalityByProductAsync(product.Id);

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetSeasonalityByProductAsync_WhenNoSeasonalityExists_ReturnsEmpty()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            var result = await repo.GetSeasonalityByProductAsync(product.Id);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSeasonalityByProductAsync_ExcludesRecordsBeyond12Months()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            // Within 12 months — should be returned
            var withinRange = await _seasonalityHelper.CreateSeasonality(
                product, 1.0m, periodStartDate: _now.AddMonths(11));

            // Beyond 12 months — should be excluded
            await _seasonalityHelper.CreateSeasonality(
                product, 1.0m, periodStartDate: _now.AddMonths(13));

            var result = await repo.GetSeasonalityByProductAsync(product.Id);

            Assert.Single(result);
            Assert.Equal(withinRange.PeriodStartDate, result.First().PeriodStartDate);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_InsertsExpectedNumberOfRows()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);

            // InsertSeasonality covers month -1 to +11 = 13 months
            int count = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(13, count);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_WhenCalledTwice_DoesNotDuplicate()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);
            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);

            int count = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(13, count);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await repo.InsertSeasonalityAsync(company.Id, [product1.Id]);

            int count1 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(13, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_WhenProductIdsNotSpecified_InsertsForAllProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await repo.InsertSeasonalityAsync(company.Id, []);

            int count1 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(13, count1);
            Assert.Equal(13, count2);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_WhenCompanySpecified_OnlyInsertsForThatCompany()
        {
            var repo = CreateRepository();

            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");

            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await repo.InsertSeasonalityAsync(company1.Id, [product1.Id]);

            int count1 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(13, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task InsertSeasonalityAsync_DefaultFactorIsOne()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);

            var records = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product.Id)
                .ToListAsync();

            Assert.All(records, s => Assert.Equal(1, s.Factor));
        }

        [Fact]
        public async Task UpdateSeasonalityAsync_WithEnoughHistory_UpdatesFactorFromDefault()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _salesHelper.CreateSalesAggregates(product, _now, 12);

            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);
            await repo.UpdateSeasonalityAsync(company.Id, [product.Id]);

            var records = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product.Id)
                .ToListAsync();

            Assert.Contains(records, s => s.Factor != 1);
        }

        [Fact]
        public async Task UpdateSeasonalityAsync_WithLessThan12MonthsHistory_KeepsFactorAtOne()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _salesHelper.CreateSalesAggregates(product, _now, 6);

            await repo.InsertSeasonalityAsync(company.Id, [product.Id]);
            await repo.UpdateSeasonalityAsync(company.Id, [product.Id]);

            var records = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product.Id)
                .ToListAsync();

            Assert.All(records, s => Assert.Equal(1, s.Factor));
        }

        [Fact]
        public async Task UpdateSeasonalityAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await _salesHelper.CreateSalesAggregates(product1, _now, 12);
            await _salesHelper.CreateSalesAggregates(product2, _now, 12);

            await repo.InsertSeasonalityAsync(company.Id, [product1.Id, product2.Id]);
            await repo.UpdateSeasonalityAsync(company.Id, [product1.Id]);

            var records1 = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product1.Id).ToListAsync();
            var records2 = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product2.Id).ToListAsync();

            Assert.Contains(records1, s => s.Factor != 1);
            Assert.All(records2, s => Assert.Equal(1, s.Factor));
        }

        [Fact]
        public async Task UpdateSeasonalityAsync_WhenCompanySpecified_OnlyUpdatesThatCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");

            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await _salesHelper.CreateSalesAggregates(product1, _now, 12);
            await _salesHelper.CreateSalesAggregates(product2, _now, 12);

            await repo.InsertSeasonalityAsync(company1.Id, [product1.Id]);
            await repo.InsertSeasonalityAsync(company2.Id, [product2.Id]);
            await repo.UpdateSeasonalityAsync(company1.Id, [product1.Id]);

            var records1 = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product1.Id).ToListAsync();
            var records2 = await Fixture.Context.Seasonality
                .Where(s => s.ProductId == product2.Id).ToListAsync();

            Assert.Contains(records1, s => s.Factor != 1);
            Assert.All(records2, s => Assert.Equal(1, s.Factor));
        }

        [Fact]
        public async Task DeleteSeasonalityAsync_DeletesPastMonths()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _seasonalityHelper.CreateSeasonality(
                product, 1.0m,
                periodStartDate: _now.AddMonths(-2));

            await repo.DeleteSeasonalityAsync(company.Id, [product.Id]);
            int count = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteSeasonalityAsync_DoesNotDeleteCurrentOrFutureMonths()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            // Current month
            await _seasonalityHelper.CreateSeasonality(product, 1.0m);

            // Future month
            await _seasonalityHelper.CreateSeasonality(
                product, 1.0m,
                periodStartDate: _now.AddMonths(3));

            await repo.DeleteSeasonalityAsync(company.Id, [product.Id]);
            int count = await Fixture.Context.Seasonality
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task DeleteSeasonalityAsync_WhenProductIdsSpecified_OnlyDeletesSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await _seasonalityHelper.CreateSeasonality(product1, 1.0m, periodStartDate: _now.AddMonths(-2));
            await _seasonalityHelper.CreateSeasonality(product2, 1.0m, periodStartDate: _now.AddMonths(-2));

            await repo.DeleteSeasonalityAsync(company.Id, [product1.Id]);
            int count1 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(1, count2);
        }

        [Fact]
        public async Task DeleteSeasonalityAsync_WhenProductIdsNotSpecified_DeletesAllProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await _seasonalityHelper.CreateSeasonality(product1, 1.0m, periodStartDate: _now.AddMonths(-2));
            await _seasonalityHelper.CreateSeasonality(product2, 1.0m, periodStartDate: _now.AddMonths(-2));

            await repo.DeleteSeasonalityAsync(company.Id, []);
            int count1 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task DeleteSeasonalityAsync_WhenCompanySpecified_OnlyDeletesThatCompany()
        {
            var repo = CreateRepository();

            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");

            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await _seasonalityHelper.CreateSeasonality(product1, 1.0m, periodStartDate: _now.AddMonths(-2));
            await _seasonalityHelper.CreateSeasonality(product2, 1.0m, periodStartDate: _now.AddMonths(-2));

            await repo.DeleteSeasonalityAsync(company1.Id, [product1.Id]);
            int count1 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.Seasonality.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(1, count2);
        }
    }
}