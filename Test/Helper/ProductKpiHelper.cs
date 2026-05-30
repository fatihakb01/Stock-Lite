using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class ProductKpiHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal async Task<ProductKpi> CreateProductKpi(
            Company company,
            int totalProductCount = 0,
            int belowSafetyStockCount = 0,
            int recommendedOrdersCount = 0,
            int withoutStockAtLeadTimeCount = 0,
            int withoutForecastCount = 0,
            JobExecutionStatus status = JobExecutionStatus.Pending,
            bool saveChanges = true
        )
        {
            var kpi = new ProductKpi
            {
                CompanyId = company.Id,
                Company = company,
                TotalProductCount = totalProductCount,
                BelowSafetyStockCount = belowSafetyStockCount,
                RecommendedOrdersCount = recommendedOrdersCount,
                WithoutStockAtLeadTimeCount = withoutStockAtLeadTimeCount,
                WithoutForecastCount = withoutForecastCount,
                SchedulerStatus = status
            };

            await _fixture.Context.ProductKpis.AddAsync(kpi);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return kpi;
        }
    }
}
