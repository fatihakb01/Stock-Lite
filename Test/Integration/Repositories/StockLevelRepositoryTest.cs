using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class StockLevelRepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private readonly ProductHelper _productHelper = new ProductHelper(fixture);
        private readonly StockLevelHelper _stockLevelHelper = new StockLevelHelper(fixture);
        private readonly ForecastHelper _forecastHelper = new ForecastHelper(fixture);
        private readonly PurchaseOrderHelper _poHelper = new PurchaseOrderHelper(fixture);
        private readonly ConfirmedOrderHelper _coHelper = new ConfirmedOrderHelper(fixture);
        private readonly InventoryPolicyHelper _policyHelper = new InventoryPolicyHelper(fixture);

        private StockLevelRepository CreateRepository() => new(Fixture.Context);

        private async Task SeedUpdatePrerequisitesAsync(
            Product product,
            int safetyStock = 10,
            int leadTimeDays = 7,
            int moq = 10,
            int orderIncrement = 10,
            decimal weeklyForecast = 1000)
        {
            var policy = await _policyHelper.CreateInventoryPolicy(
                product,
                safetyStock: safetyStock,
                leadTimeDays: leadTimeDays,
                minimumOrderQuantity: moq,
                orderIncrementQuantity: orderIncrement);

            await _forecastHelper.SeedWeeklyForecastsAsync(product: product, predictedQuantity: weeklyForecast);

            var repo = new PurchaseForecastRepository(Fixture.Context);
            await repo.InsertPurchaseForecastsAsync(product.CompanyId, [product.Id]);
            await repo.UpdatePurchaseForecastsAsync(product.CompanyId, [product.Id]);

            await Fixture.Context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByProductAsync_WhenStockLevelExists_ReturnsStockLevel()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _stockLevelHelper.CreateStockLevel(product);

            var result = await repo.GetByProductAsync(product.Id);

            Assert.NotNull(result);
            Assert.Equal(product.Id, result!.ProductId);
        }

        [Fact]
        public async Task GetByProductAsync_WhenNoStockLevelExists_ReturnsNull()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            var result = await repo.GetByProductAsync(product.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByProductAsync_IncludesProduct()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _stockLevelHelper.CreateStockLevel(product);

            var result = await repo.GetByProductAsync(product.Id);

            Assert.NotNull(result!.Product);
            Assert.Equal(product.SKU, result.Product.SKU);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_InsertsOneRowPerProduct()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);

            int count = await Fixture.Context.StockLevels
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_WhenCalledTwice_DoesNotDuplicate()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);

            int count = await Fixture.Context.StockLevels
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_InitializesValuesToZero()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);

            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.Equal(0, stockLevel.QuantityOnHand);
            Assert.Equal(0, stockLevel.QuantityOnOrder);
            Assert.Equal(0, stockLevel.QuantityCommitted);
            Assert.Equal(0, stockLevel.NetStockPosition);
            Assert.Equal(0, stockLevel.ForecastQuantity);
            Assert.Equal(0, stockLevel.RecommendedOrderQuantity);
            Assert.False(stockLevel.NeedsReorder);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await repo.InsertStockLevelsAsync(company.Id, [product1.Id]);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(1, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_WhenProductIdsNotSpecified_InsertsForAllProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await repo.InsertStockLevelsAsync(company.Id, []);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(1, count1);
            Assert.Equal(1, count2);
        }

        [Fact]
        public async Task InsertStockLevelsAsync_WhenCompanySpecified_OnlyInsertsForThatCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");
            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await repo.InsertStockLevelsAsync(company1.Id, [product1.Id]);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(1, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_SetsReorderPointToSafetyStock()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await SeedUpdatePrerequisitesAsync(product, safetyStock: 25);
            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product.Id]);

            Fixture.Context.ChangeTracker.Clear();
            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.Equal(25, stockLevel.ReorderPoint);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WithLowStock_SetsNeedsReorderTrue()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            // High safety stock, no received POs = QuantityOnHand 0 < SafetyStock 100
            await SeedUpdatePrerequisitesAsync(product, safetyStock: 100);
            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product.Id]);

            Fixture.Context.ChangeTracker.Clear();
            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.True(stockLevel.NeedsReorder);
            Assert.True(stockLevel.RecommendedOrderQuantity > 0);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WithSufficientStock_NeedsReorderIsFalse()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await SeedUpdatePrerequisitesAsync(product: product, leadTimeDays: 1, safetyStock: 0, weeklyForecast: 0);

            // Seed received purchase orders to create QuantityOnHand
            var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
            var po = await _poHelper.CreatePurchaseOrder(
                company, 
                status: PurchaseOrderStatus.Completed, 
                expectedDeliveryDate: startDate.AddDays(2),
                receivedDate: startDate);

            var poLine = await _poHelper.CreatePurchaseOrderLineItem(
                po, product, quantity: 1000, receivedQuantity: 1000);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product.Id]);

            Fixture.Context.ChangeTracker.Clear();
            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.False(stockLevel.NeedsReorder);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WithOpenPurchaseOrders_PopulatesQuantityOnOrder()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await SeedUpdatePrerequisitesAsync(product);

            var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);

            var po = await _poHelper.CreatePurchaseOrder(
                company,
                expectedDeliveryDate: startDate.AddDays(3),
                status: PurchaseOrderStatus.Draft);

            await _poHelper.CreatePurchaseOrderLineItem(
                po, product, quantity: 50, receivedQuantity: 0);

            // Re-run purchase forecast update so it picks up the new PO
            var pfRepo = new PurchaseForecastRepository(Fixture.Context);
            await pfRepo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product.Id]);

            Fixture.Context.ChangeTracker.Clear();
            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.True(stockLevel.QuantityOnOrder > 0);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WithOpenConfirmedOrders_PopulatesQuantityOnCommitted()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await SeedUpdatePrerequisitesAsync(product);

            var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
            var co = await _coHelper.CreateConfirmedOrder(
                company,
                expectedDeliveryDate: startDate.AddDays(3),
                status: ConfirmedOrderStatus.Confirmed);

            await _coHelper.CreateConfirmedOrderLineItem(
                co,
                product,
                quantity: 50,
                soldQuantity: 0);

            // Re-run purchase forecast update so it picks up the new PO
            var pfRepo = new PurchaseForecastRepository(Fixture.Context);
            await pfRepo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

            await repo.InsertStockLevelsAsync(company.Id, [product.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product.Id]);

            Fixture.Context.ChangeTracker.Clear();
            var stockLevel = await Fixture.Context.StockLevels
                .FirstAsync(s => s.ProductId == product.Id);

            Assert.True(stockLevel.QuantityCommitted > 0);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await SeedUpdatePrerequisitesAsync(product1, safetyStock: 100);
            await SeedUpdatePrerequisitesAsync(product2, safetyStock: 100);

            await repo.InsertStockLevelsAsync(company.Id, [product1.Id, product2.Id]);
            await repo.UpdateStockLevelsAsync(company.Id, [product1.Id]);

            Fixture.Context.ChangeTracker.Clear();

            var sl1 = await Fixture.Context.StockLevels.FirstAsync(s => s.ProductId == product1.Id);
            var sl2 = await Fixture.Context.StockLevels.FirstAsync(s => s.ProductId == product2.Id);

            Assert.Equal(100, sl1.ReorderPoint);
            Assert.Equal(0, sl2.ReorderPoint);
        }

        [Fact]
        public async Task UpdateStockLevelsAsync_WhenCompanySpecified_OnlyUpdatesThatCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");
            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await SeedUpdatePrerequisitesAsync(product1, safetyStock: 50);
            await SeedUpdatePrerequisitesAsync(product2, safetyStock: 50);

            await repo.InsertStockLevelsAsync(company1.Id, [product1.Id]);
            await repo.InsertStockLevelsAsync(company2.Id, [product2.Id]);
            await repo.UpdateStockLevelsAsync(company1.Id, [product1.Id]);

            Fixture.Context.ChangeTracker.Clear();

            var sl1 = await Fixture.Context.StockLevels.FirstAsync(s => s.ProductId == product1.Id);
            var sl2 = await Fixture.Context.StockLevels.FirstAsync(s => s.ProductId == product2.Id);

            Assert.Equal(50, sl1.ReorderPoint);
            Assert.Equal(0, sl2.ReorderPoint); 
        }

        [Fact]
        public async Task DeleteStockLevelsAsync_DeletesStockLevel()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product = await _productHelper.CreateProduct(company);

            await _stockLevelHelper.CreateStockLevel(product);

            await repo.DeleteStockLevelsAsync(company.Id, [product.Id]);

            int count = await Fixture.Context.StockLevels
                .CountAsync(s => s.ProductId == product.Id);

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteStockLevelsAsync_WhenProductIdsSpecified_OnlyDeletesSpecifiedProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await _stockLevelHelper.CreateStockLevel(product1);
            await _stockLevelHelper.CreateStockLevel(product2);

            await repo.DeleteStockLevelsAsync(company.Id, [product1.Id]);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(1, count2);
        }

        [Fact]
        public async Task DeleteStockLevelsAsync_WhenProductIdsNotSpecified_DeletesAllProducts()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();
            var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

            await _stockLevelHelper.CreateStockLevel(product1);
            await _stockLevelHelper.CreateStockLevel(product2);

            await repo.DeleteStockLevelsAsync(company.Id, []);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(0, count2);
        }

        [Fact]
        public async Task DeleteStockLevelsAsync_WhenCompanySpecified_OnlyDeletesThatCompany()
        {
            var repo = CreateRepository();
            var company1 = await _companyHelper.CreateCompany(name: "Company 1");
            var company2 = await _companyHelper.CreateCompany(name: "Company 2");
            var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
            var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

            await _stockLevelHelper.CreateStockLevel(product1);
            await _stockLevelHelper.CreateStockLevel(product2);

            await repo.DeleteStockLevelsAsync(company1.Id, [product1.Id]);

            int count1 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product1.Id);
            int count2 = await Fixture.Context.StockLevels.CountAsync(s => s.ProductId == product2.Id);

            Assert.Equal(0, count1);
            Assert.Equal(1, count2);
        }
    }
}