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
        CreateMap<Transaction, EditTransactionDto>();
        CreateMap<CreateTransactionDto, Transaction>();
        CreateMap<EditTransactionDto, Transaction>();

        // Product entity
        CreateMap<CreateProductDto, Product>();
        CreateMap<EditProductDto, Product>();
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockLevel));
        CreateMap<StockLevel, ProductStockDto>();

        // TransactionLineItem entity
        CreateMap<TransactionLineItem, TransactionLineItemDto>();
        CreateMap<CreateTransactionLineItemDto, TransactionLineItem>();
        CreateMap<EditTransactionLineItemDto, TransactionLineItem>();

        //SalesAggregate entity
        CreateMap<SalesAggregate, SalesAggregateDto>();

        // User entity
        CreateMap<User, UserDto>();
        CreateMap<RegisterUserDto, User>();
        CreateMap<LoginUserDto, User>();
        CreateMap<ConfirmEmailDto, User>();
        // CreateMap<ChangePasswordDto, User>();
        // CreateMap<ForgotPasswordDto, User>();
    }
}
