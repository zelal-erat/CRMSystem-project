using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using MediatR;

namespace CRMSystem.Application.Commands.Invoice;

public record UpdateInvoiceCommand(UpdateInvoiceDto Invoice) : IRequest<Result<InvoiceDetailDto>>;
