using AutoMapper;
using Domain.Entities.Core;
using Domain.Entities.Derived;
using Application.Entities.Core.Companies.DTOs;
using Application.Entities.Core.Transactions.DTOs;
using Application.Entities.Core.Products.DTOs;
using Application.Entities.Core.TransactionLineItems.DTOs;
using Application.Entities.Derived.SalesAggregates.DTOs;
using Application.Entities.Core.Users.DTOs;
using Application.Entities.Derived.Forecasts.DTOs;
using Application.Entities.Core.PurchaseOrders.DTOs;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using Application.Entities.Derived.ConfirmedOrderAggregates.DTOs;
using Application.Entities.Derived.StockLevels.DTOs;
using Application.Entities.Derived.PurchaseForecasts.DTOs;
using Application.Entities.Core.JobTasks.DTOs;
using Application.Entities.Core.ScheduledJobs.DTOs;
using Application.Entities.Derived.ProductKpis.DTOs;
using Application.Entities.Derived.Seasonality.DTOs;
using Application.Entities.Core.UserDefinedFields.DTOs;
using Application.Entities.Core.ExportProfiles.DTOs;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Company entity
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<EditCompanyDto, Company>();

        // Transaction entity
        CreateMap<Transaction, TransactionDto>();
        CreateMap<CreateTransactionDto, Transaction>();
        CreateMap<EditTransactionDto, Transaction>();

        // TransactionLineItem entity
        CreateMap<TransactionLineItem, TransactionLineItemDto>();
        CreateMap<CreateTransactionLineItemDto, TransactionLineItem>();
        CreateMap<EditTransactionLineItemDto, TransactionLineItem>();

        // Product entity
        CreateMap<CreateProductDto, Product>();
        CreateMap<EditProductDto, Product>();
        CreateMap<Product, ProductDetailDto>();
        CreateMap<Product, ProductListDto>();
        CreateMap<StockLevel, ProductStockDto>();
        CreateMap<ProductInventoryPolicy, ProductInventoryPolicyDto>();
        CreateMap<ProductKpi, ProductKpiDto>();

        // PurchaseOrder entity
        CreateMap<PurchaseOrder, PurchaseOrderDto>();
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();
        CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();

        // PurchaseOrderLineItem entity
        CreateMap<PurchaseOrderLineItem, PurchaseOrderLineItemDto>();
        CreateMap<CreatePurchaseOrderLineItemDto, PurchaseOrderLineItem>();
        CreateMap<UpdatePurchaseOrderLineItemDto, PurchaseOrderLineItem>();

        // ConfirmedOrder entity
        CreateMap<ConfirmedOrder, ConfirmedOrderDto>();
        CreateMap<CreateConfirmedOrderDto, ConfirmedOrder>();
        CreateMap<UpdateConfirmedOrderDto, ConfirmedOrder>();

        // ConfirmedOrderLineItem entity
        CreateMap<ConfirmedOrderLineItem, ConfirmedOrderLineItemDto>();
        CreateMap<CreateConfirmedOrderLineItemDto, ConfirmedOrderLineItem>();
        CreateMap<UpdateConfirmedOrderLineItemDto, ConfirmedOrderLineItem>();

        // SalesAggregate entity
        CreateMap<SalesAggregate, SalesAggregateDto>();

        // ConfirmedOrderAggregate entity
        CreateMap<ConfirmedOrderAggregate, ConfirmedOrderAggregateDto>();

        // Forecast entity
        CreateMap<Forecast, ForecastDto>();

        // Seasonality entity
        CreateMap<Seasonality, SeasonalityDto>();

        // PurchaseForecast entity
        CreateMap<PurchaseForecast, PurchaseForecastDto>();

        // Stocklevel entity
        CreateMap<StockLevel, StockLevelDto>();

        // UserDefinedField entity
        CreateMap<UserDefinedField, UserDefinedFieldDto>();

        // ExportProfile
        CreateMap<ExportProfile, ExportProfileDto>();
        CreateMap<ExportProfile, ExportProfileDetailDto>();
        CreateMap<CreateExportProfileDto, ExportProfile>();
        CreateMap<EditExportProfileDto, ExportProfile>();

        // User entity
        CreateMap<User, CurrentUserDto>();
        CreateMap<User, UserDto>();
        CreateMap<RegisterUserDto, User>();
        CreateMap<LoginUserDto, User>();
        CreateMap<ConfirmEmailDto, User>();
        CreateMap<EditUserDto, User>();

        // JobTask entity
        CreateMap<JobTask, JobTaskDto>();
        CreateMap<JobTask, JobTaskDetailDto>();
        CreateMap<CreateJobTaskDto, JobTask>();
        CreateMap<EditJobTaskDto, JobTask>();

        // ScheduledJob entity
        CreateMap<ScheduledJob, ScheduledJobDto>();
        CreateMap<CreateScheduledJobDto, ScheduledJob>();
        CreateMap<EditScheduledJobDto, ScheduledJob>();

        // JobExecution entity
        CreateMap<JobExecution, JobExecutionDto>();
    }
}
