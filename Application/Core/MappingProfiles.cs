using AutoMapper;
using Domain.Entities;
using Application.Entities.Companies.DTOs;
using Application.Entities.Customers.DTOs;
using Application.Entities.Transactions.DTOs;
using Application.Entities.Products.DTOs;
using Application.Entities.TransactionLineItems.DTOs;


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
    }
}
