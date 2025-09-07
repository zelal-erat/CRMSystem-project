using CRMSystem.Application.DTOs.Common;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Queries.Customers;

public record GetCustomersQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedResultDto<CustomerDto>>>;
