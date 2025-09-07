using CRMSystem.Application.Commands.Invoice;
using CRMSystem.Application.Common;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;

namespace CRMSystem.Application.Handlers.Invoices;

public class DeleteInvoiceHandler : IRequestHandler<DeleteInvoiceCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public DeleteInvoiceHandler(IUnitOfWork unitOfWork, IInvoiceDomainService invoiceDomainService)
    {
        _unitOfWork = unitOfWork;
        _invoiceDomainService = invoiceDomainService;
    }

    public async Task<Result<bool>> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Domain Service kullanarak invoice sil
        var deleteResult = await _invoiceDomainService.DeleteInvoiceAsync(request.Id);
        
        if (!deleteResult.IsSuccess)
            return Result<bool>.Failure(deleteResult.Error);

        return Result<bool>.Success(true);
    }
}
