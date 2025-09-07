using System.Linq.Expressions;
using CRMSystem.Application.Common;
using CRMSystem.Application.Common.Exceptions;
using CRMSystem.Application.DTOs.Common;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Customers;

public class GetCustomersQueryHandler : IRequestHandler<CRMSystem.Application.Queries.Customers.GetCustomersQuery, Result<PaginatedResultDto<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResultDto<CustomerDto>>> Handle(CRMSystem.Application.Queries.Customers.GetCustomersQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Customer, bool>> predicate = c => true;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            predicate = c => c.FullName.ToLower().Contains(term) || c.Email.ToLower().Contains(term);
        }

        var items = await _unitOfWork.Customers.GetPagedAsync(request.PageNumber, request.PageSize, predicate);
        var total = await _unitOfWork.Customers.CountAsync(predicate);

        var dtoItems = _mapper.Map<List<CustomerDto>>(items);

        var result = new PaginatedResultDto<CustomerDto>
        {
            Items = dtoItems,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber * request.PageSize < total
        };

        return Result<PaginatedResultDto<CustomerDto>>.Success(result);
    }
}
