using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Invoices;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<InvoiceDetailDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Invoices.GetInvoiceWithItemsAsync(request.Id);
        
        if (invoice == null)
        {
            return Result<InvoiceDetailDto>.Failure("Fatura bulunamadı.");
        }

        // Business Rule: Fatura detayı getirilirken durum kontrolü yap
        await _invoiceDomainService.CheckAndUpdateInvoiceStatusAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<InvoiceDetailDto>(invoice);
        return Result<InvoiceDetailDto>.Success(dto);
    }
}