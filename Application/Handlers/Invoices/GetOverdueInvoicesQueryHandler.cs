using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Invoices;

public class GetOverdueInvoicesQueryHandler : IRequestHandler<GetOverdueInvoicesQuery, Result<IEnumerable<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOverdueInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> Handle(GetOverdueInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();
        var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        return Result<IEnumerable<InvoiceDto>>.Success(dtos);
    }
}
