using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Invoices;

public class GetUpcomingInvoicesQueryHandler : IRequestHandler<GetUpcomingInvoicesQuery, Result<IEnumerable<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUpcomingInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> Handle(GetUpcomingInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _unitOfWork.Invoices.GetUpcomingInvoicesAsync();
        var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        return Result<IEnumerable<InvoiceDto>>.Success(dtos);
    }
}
