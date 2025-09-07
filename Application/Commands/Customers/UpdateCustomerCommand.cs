using MediatR;
using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Customer;

namespace CRMSystem.Application.Commands.Customers;

public record UpdateCustomerCommand(int Id, UpdateCustomerDto Customer) : IRequest<Result<CustomerDto>>;
