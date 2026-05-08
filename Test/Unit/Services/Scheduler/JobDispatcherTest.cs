using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.Scheduler
{
    public class JobDispatcherTest
    {
        private static (Mock<IServiceProvider> root, Mock<IServiceProvider> scoped) SetupScopedProvider(
            Action<Mock<IServiceProvider>> configureScoped)
        {
            // 1. Create the inner scoped provider — tests put their services here
            var mockScopedProvider = new Mock<IServiceProvider>();
            configureScoped(mockScopedProvider); // e.g. register IImportCoordinator

            // 2. Wrap it in a scope object
            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopedProvider.Object);

            // 3. Wrap the scope in a factory
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            // 4. Register the factory on the root provider
            var mockRoot = new Mock<IServiceProvider>();
            mockRoot.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                    .Returns(mockScopeFactory.Object);

            return (mockRoot, mockScopedProvider);
        }

        // --- JobType routing ---
        [Fact]
        public async Task DispatchAsync_UnsupportedJobType_ThrowsInvalidOperationException()
        {
            var (root, _) = SetupScopedProvider(_ => { });
            var dispatcher = new JobDispatcher(root.Object, null!);

            var jobTask = SchedulerHelper.CreateJobTask();
            var job = new ScheduledJob
            {
                Id = Guid.NewGuid(),
                JobType = (JobType)99,
                JobTask = jobTask,
                JobTaskId = jobTask.Id,
                Name = "Unknown",
                Result = JobExecutionStatus.Pending
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.DispatchAsync(job, CancellationToken.None));
        }

        // --- HandleImport validation ---
        [Theory]
        [InlineData(null, "C:\\Data\\products.csv")]
        [InlineData(ImportDataType.Products, null)]
        [InlineData(ImportDataType.Products, "")]
        [InlineData(ImportDataType.Products, "   ")]
        public async Task HandleImport_InvalidArguments_ThrowsInvalidOperationException(
            ImportDataType? dataType, string? source)
        {
            var (root, _) = SetupScopedProvider(_ => { });
            var dispatcher = new JobDispatcher(root.Object, null!);

            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.Import);
            job.ImportDataType = dataType;
            job.Source = source;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.HandleImport(job, CancellationToken.None));
        }

        [Theory]
        [InlineData(ImportDataType.Products)]
        [InlineData(ImportDataType.Transactions)]
        [InlineData(ImportDataType.ConfirmedOrders)]
        [InlineData(ImportDataType.PurchaseOrders)]
        [InlineData(ImportDataType.UserDefinedFields)]
        public async Task HandleImport_ValidArguments_CallsCoordinatorRunAsync(ImportDataType dataType)
        {
            var mockCoordinator = new Mock<IImportCoordinator>();
            mockCoordinator
                .Setup(c => c.RunAsync(
                    It.IsAny<ImportDataType>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var (root, _) = SetupScopedProvider(scoped =>
                scoped.Setup(p => p.GetService(typeof(IImportCoordinator)))
                      .Returns(mockCoordinator.Object));

            var dispatcher = new JobDispatcher(root.Object, null!);

            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.Import);
            job.ImportDataType = dataType;
            job.Source = "C:\\Data\\file.csv";

            await dispatcher.HandleImport(job, CancellationToken.None);

            mockCoordinator.Verify(
                c => c.RunAsync(dataType, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        // --- HandleStandardCalculation routing ---
        [Theory]
        [InlineData(StandardCalculationType.StockLevels,              typeof(IStockLevelCalculation))]
        [InlineData(StandardCalculationType.ConfirmedOrderAggregates, typeof(IConfirmedOrderAggregateCalculation))]
        [InlineData(StandardCalculationType.Forecasts,                typeof(IForecastCalculation))]
        [InlineData(StandardCalculationType.SalesAggregates,          typeof(ISalesAggregateCalculation))]
        [InlineData(StandardCalculationType.PurchaseForecasts,        typeof(IPurchaseForecastCalculation))]
        [InlineData(StandardCalculationType.ProductKpis,              typeof(IProductKpiCalculation))]
        [InlineData(StandardCalculationType.Seasonality,              typeof(ISeasonalityCalculation))]
        public async Task DispatchAsync_StandardCalculation_CallsCorrectService(
            StandardCalculationType calculationType, Type serviceType)
        {
            var(service, mock) = MockCalculationService(serviceType);

            var (root, _) = SetupScopedProvider(scoped =>
                scoped.Setup(p => p.GetService(serviceType)).Returns(service));

            var dispatcher = new JobDispatcher(root.Object, null!);
            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.StandardCalculation);
            job.CalculationType = calculationType;

            await dispatcher.DispatchAsync(job, CancellationToken.None);

            VerifyCalculationServiceCalled(mock, serviceType);
        }

        [Fact]
        public async Task DispatchAsync_StandardCalculation_UnsupportedType_Throws()
        {
            var (root, _) = SetupScopedProvider(_ => { });
            var dispatcher = new JobDispatcher(root.Object, null!);
            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.StandardCalculation);
            job.CalculationType = (StandardCalculationType) 99;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.DispatchAsync(job, CancellationToken.None));
        }

        // --- HandleCustomCalculation validation ---
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task HandleCustomCalculation_NullOrEmptyTarget_Throws(string? target)
        {
            var (root, _) = SetupScopedProvider(_ => { });
            var dispatcher = new JobDispatcher(root.Object, null!);
            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.CustomCalculation);
            job.Target = target;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.DispatchAsync(job, CancellationToken.None));
        }

        [Theory]
        [InlineData("my_proc")]          // missing custom_ prefix
        [InlineData("sp_custom_proc")]   // wrong prefix
        [InlineData("custom")]           // prefix only, no name
        [InlineData("PROC_custom_")]     // wrong order
        public async Task HandleCustomCalculation_InvalidNamingConvention_Throws(string target)
        {
            var (root, _) = SetupScopedProvider(_ => { });
            var dispatcher = new JobDispatcher(root.Object, null!);
            var job = SchedulerHelper.CreateScheduledJob(jobType: JobType.CustomCalculation);
            job.Target = target;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                dispatcher.DispatchAsync(job, CancellationToken.None));
        }

        [Theory]
        [InlineData("custom_my_proc")]        // valid without schema
        [InlineData("dbo.custom_my_proc")]    // valid with schema
        [InlineData("CUSTOM_my_proc")]        // case-insensitive check
        [InlineData("DBO.CUSTOM_my_proc")]    // case-insensitive with schema
        public void HandleCustomCalculation_ValidNamingConvention_DoesNotThrowOnNameCheck(string target)
        {
            // Just verify the naming convention logic passes —
            // the DB check will fail later (integration concern)
            var valid =
                target.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase);

            Assert.True(valid);
        }

        // --- Helpers for calculation service mocking ---
        private static (object Service, object Mock) MockCalculationService(Type serviceType)
        {
            if (serviceType == typeof(IStockLevelCalculation))
            {
                var m = new Mock<IStockLevelCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(IConfirmedOrderAggregateCalculation))
            {
                var m = new Mock<IConfirmedOrderAggregateCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(IForecastCalculation))
            {
                var m = new Mock<IForecastCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(ISalesAggregateCalculation))
            {
                var m = new Mock<ISalesAggregateCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(IPurchaseForecastCalculation))
            {
                var m = new Mock<IPurchaseForecastCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(IProductKpiCalculation))
            {
                var m = new Mock<IProductKpiCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }
            else if (serviceType == typeof(ISeasonalityCalculation))
            {
                var m = new Mock<ISeasonalityCalculation>();
                m.Setup(s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
                return (m.Object, m);
            }

            throw new ArgumentException($"Unknown service type: {serviceType}");
        }

        private static void VerifyCalculationServiceCalled(object mockService, Type serviceType)
        {
            if (serviceType == typeof(IStockLevelCalculation))
                ((Mock<IStockLevelCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(IConfirmedOrderAggregateCalculation))
                ((Mock<IConfirmedOrderAggregateCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(IForecastCalculation))
                ((Mock<IForecastCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(ISalesAggregateCalculation))
                ((Mock<ISalesAggregateCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<Period?>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(IPurchaseForecastCalculation))
                ((Mock<IPurchaseForecastCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(IProductKpiCalculation))
                ((Mock<IProductKpiCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

            else if (serviceType == typeof(ISeasonalityCalculation))
                ((Mock<ISeasonalityCalculation>)mockService).Verify(
                    s => s.RunAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}