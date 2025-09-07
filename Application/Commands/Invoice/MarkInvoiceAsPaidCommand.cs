using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Commands.Invoice;

public record MarkInvoiceAsPaidCommand(int Id) : IRequest<Result<bool>>;
