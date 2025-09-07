using MediatR;
using CRMSystem.Application.Common;

namespace CRMSystem.Application.Commands.Customers;

public record DeleteCustomerCommand(int Id) : IRequest<Result<bool>>;

