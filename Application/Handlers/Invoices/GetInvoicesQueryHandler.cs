using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Invoices;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<IEnumerable<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public GetInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Invoices.GetInvoicesWithItemsAsync();
        
        // Business Rule: Fatura listesi getirilirken durum kontrolü yap
        foreach (var invoice in items)
        {
            await _invoiceDomainService.CheckAndUpdateInvoiceStatusAsync(invoice);
        }
        await _unitOfWork.SaveChangesAsync();
        
        var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(items);
        return Result<IEnumerable<InvoiceDto>>.Success(dtos);
    }
}
