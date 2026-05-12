using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class ProductCompanyHelper(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private ProductRepository CreateRepository()
            => new ProductRepository(Fixture.Context);

        [Fact]
        public async Task GetFilteredProducts_WhenFilterIsNull_ReturnsAllProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                null,
                company.Id,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetFilteredProducts_BelowSafetyStock_ReturnsMatchingProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                ProductFilter.BelowSafetyStock,
                company.Id,
                CancellationToken.None);

            Assert.Single(result);

            var product = result.Single();

            Assert.Equal("SKU-0002", product.SKU);
            Assert.NotNull(product.InventoryPolicy);
            Assert.NotNull(product.StockLevel);
            Assert.True(
                product.StockLevel!.QuantityOnHand <
                product.InventoryPolicy!.SafetyStock);
        }

        [Fact]
        public async Task GetFilteredProducts_WithRecommendedOrders_ReturnsMatchingProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                ProductFilter.WithRecommendedOrders,
                company.Id,
                CancellationToken.None);

            Assert.Single(result);

            var product = result.Single();

            Assert.Equal("SKU-0001", product.SKU);
            Assert.NotNull(product.StockLevel);
            Assert.True(product.StockLevel!.RecommendedOrderQuantity > 0);
        }

        [Fact]
        public async Task GetFilteredProducts_WithoutStockAtLeadTime_ReturnsMatchingProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                ProductFilter.WithoutStockAtLeadTime,
                company.Id,
                CancellationToken.None);

            Assert.Single(result);

            var product = result.Single();

            Assert.Equal("SKU-0001", product.SKU);
            Assert.NotNull(product.StockLevel);
            Assert.True(product.StockLevel!.NetStockPosition <= 0);
        }

        [Fact]
        public async Task GetFilteredProducts_WithoutForecast_ReturnsMatchingProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var products = ProductHelper.CreateProducts(company);

            await Fixture.Context.Products.AddRangeAsync(products);
            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                ProductFilter.WithoutForecast,
                company.Id,
                CancellationToken.None);

            Assert.Single(result);

            var product = result.Single();

            Assert.Equal("SKU-0001", product.SKU);
            Assert.NotNull(product.StockLevel);
            Assert.True(product.StockLevel!.ForecastQuantity <= 0);
        }

        [Fact]
        public async Task GetFilteredProducts_ReturnsOnlyProductsForCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany();
            var company2 = await _companyHelper.CreateCompany();

            var company1Products = ProductHelper.CreateProducts(company1);
            var company2Products = ProductHelper.CreateProducts(company2);

            await Fixture.Context.Products.AddRangeAsync(company1Products);
            await Fixture.Context.Products.AddRangeAsync(company2Products);

            await Fixture.Context.SaveChangesAsync();

            var result = await repo.GetFilteredProducts(
                null,
                company1.Id,
                CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(company1.Id, p.CompanyId));
        }

        [Fact]
        public async Task GetFilteredProducts_WhenNoProductsExist_ReturnsEmptyList()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var result = await repo.GetFilteredProducts(
                ProductFilter.BelowSafetyStock,
                company.Id,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}