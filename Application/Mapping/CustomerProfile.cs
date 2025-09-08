using AutoMapper;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Entities;
using CRMSystem.Application.Commands.Customers;
using CRMSystem.Domain.Services;

namespace CRMSystem.Application.Mapping;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Entity -> DTO
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerDetailDto>();
        
        // Invoice -> CustomerInvoiceDto mapping
        CreateMap<Invoice, CustomerInvoiceDto>();
        
        // DTO -> Entity
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        
        // DTO -> Domain Request (Handler'ları sadeleştirmek için)
        CreateMap<CreateCustomerDto, CreateCustomerRequest>();
        CreateMap<UpdateCustomerDto, UpdateCustomerRequest>();
    }
}
