using AutoMapper;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Entities;
using CRMSystem.Application.Commands.Customers;

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
    }
}
