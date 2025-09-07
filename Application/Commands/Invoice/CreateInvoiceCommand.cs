using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using MediatR;

namespace CRMSystem.Application.Commands.Invoice;

public record CreateInvoiceCommand(CreateInvoiceDto Invoice) : IRequest<Result<InvoiceDetailDto>>;
