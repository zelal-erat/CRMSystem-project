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

public class UpdateInvoiceHandler : IRequestHandler<UpdateInvoiceCommand, Result<InvoiceDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public UpdateInvoiceHandler(IUnitOfWork unitOfWork, IMapper mapper, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<InvoiceDetailDto>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Domain Service kullanarak invoice güncelle
        var items = request.Invoice.Items.Select(item => new CreateInvoiceItemRequest
        {
            ServiceId = item.ServiceId,
            RenewalCycle = Enum.Parse<RenewalCycle>(item.RenewalCycle),
            Price = item.Price,
            Quantity = item.Quantity,
            VAT = item.VAT,
            StartDate = item.StartDate,
            Description = item.Description
        }).ToList();

        var invoiceResult = await _invoiceDomainService.UpdateInvoiceAsync(
            request.Invoice.Id,
            request.Invoice.CustomerId,
            request.Invoice.Description,
            request.Invoice.Status,
            items);

        if (!invoiceResult.IsSuccess)
            return Result<InvoiceDetailDto>.Failure(invoiceResult.Error);

        // Güncellenmiş faturayı döndür
        var updatedInvoice = await _unitOfWork.Invoices.GetInvoiceWithItemsAsync(invoiceResult.Data.Id);
        var dto = _mapper.Map<InvoiceDetailDto>(updatedInvoice);
        return Result<InvoiceDetailDto>.Success(dto);
    }
}
