using CRMSystem.Application.Common;
using CRMSystem.Application.Commands.Invoice;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;

namespace CRMSystem.Application.Handlers.Invoices;

public class MarkInvoiceAsPaidCommandHandler : IRequestHandler<MarkInvoiceAsPaidCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public MarkInvoiceAsPaidCommandHandler(IUnitOfWork unitOfWork, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<bool>> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        // Domain Service kullanarak invoice'ı ödenmiş olarak işaretle
        var markPaidResult = await _invoiceDomainService.MarkInvoiceAsPaidAsync(request.Id);
        
        if (!markPaidResult.IsSuccess)
            return Result<bool>.Failure(markPaidResult.Error);

        return Result<bool>.Success(true);
    }
}
