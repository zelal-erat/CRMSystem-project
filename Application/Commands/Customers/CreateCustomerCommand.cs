using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Commands.Customers;

public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<Result<CustomerDto>>;
