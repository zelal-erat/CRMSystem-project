using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Queries.Customers;

public record GetCustomerByIdQuery(int Id) : IRequest<Result<CustomerDetailDto?>>;
