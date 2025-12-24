using AutoMapper;
using Domain.Entities.Core;
using Domain.Entities.Derived;
using Application.Entities.Core.Companies.DTOs;
using Application.Entities.Core.Customers.DTOs;
using Application.Entities.Core.Transactions.DTOs;
using Application.Entities.Core.Products.DTOs;
using Application.Entities.Core.TransactionLineItems.DTOs;
using Application.Entities.Derived.SalesAggregates.DTOs;
using Application.Entities.Core.Users.DTOs;
using Application.Entities.Derived.Forecasts.DTOs;
using Application.Entities.Core.PurchaseOrders.DTOs;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Company entity
        CreateMap<Company, CompanyDto>();
        CreateMap<Company, EditCompanyDto>();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<EditCompanyDto, Company>();

        // Customer entity
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, EditCustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<EditCustomerDto, Customer>();

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
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockLevel));
        CreateMap<StockLevel, ProductStockDto>();

        // PurchaseOrder entity
        CreateMap<PurchaseOrder, PurchaseOrderDto>();
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();
        CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();

        // PurchaseOrderLineItem entity
        CreateMap<PurchaseOrderLineItem, PurchaseOrderLineItemDto>();
        CreateMap<CreatePurchaseOrderLineItemDto, PurchaseOrderLineItem>();
        CreateMap<UpdatePurchaseOrderLineItemDto, PurchaseOrderLineItem>();

        // SalesAggregate entity
        CreateMap<SalesAggregate, SalesAggregateDto>();

        // Forecast entity
        CreateMap<Forecast, ForecastDto>();

        // User entity
        CreateMap<User, UserDto>();
        CreateMap<RegisterUserDto, User>();
        CreateMap<LoginUserDto, User>();
        CreateMap<ConfirmEmailDto, User>();
        CreateMap<EditUserDto, User>();
    }
}
