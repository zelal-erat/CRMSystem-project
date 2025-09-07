using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Commands.Invoice;

public record DeleteInvoiceCommand(int Id) : IRequest<Result<bool>>;
