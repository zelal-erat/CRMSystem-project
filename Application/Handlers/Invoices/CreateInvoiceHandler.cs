using CRMSystem.Application.Commands.Invoice;
using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Enums;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Invoices;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public CreateInvoiceHandler(IUnitOfWork unitOfWork, IMapper mapper, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<InvoiceDetailDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // AutoMapper kullanarak DTO'dan Request'e mapping
        var items = _mapper.Map<List<CreateInvoiceItemRequest>>(request.Invoice.Items);

        var invoiceResult = await _invoiceDomainService.CreateInvoiceAsync(
            request.Invoice.CustomerId, 
            request.Invoice.Description, 
            items);

        if (!invoiceResult.IsSuccess)
            return Result<InvoiceDetailDto>.Failure(invoiceResult.Error);

        // Detaylı DTO döndür
        var invoiceWithItems = await _unitOfWork.Invoices.GetInvoiceWithItemsAsync(invoiceResult.Data.Id);
        var dto = _mapper.Map<InvoiceDetailDto>(invoiceWithItems);
        return Result<InvoiceDetailDto>.Success(dto);
    }
}
