using CRMSystem.Application.Common;
using CRMSystem.Application.Common.Exceptions;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Customers;

public class GetCustomerByIdQueryHandler : IRequestHandler<CRMSystem.Application.Queries.Customers.GetCustomerByIdQuery, Result<CustomerDetailDto?>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDetailDto?>> Handle(CRMSystem.Application.Queries.Customers.GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted);
        if (customer is null) 
            return Result<CustomerDetailDto?>.Success(null);

        var invoices = await _unitOfWork.Invoices.GetInvoicesByCustomerIdAsync(customer.Id);

        var dto = _mapper.Map<CustomerDetailDto>(customer);
        dto.Invoices = _mapper.Map<List<CustomerInvoiceDto>>(invoices);

        return Result<CustomerDetailDto?>.Success(dto);
    }
}
