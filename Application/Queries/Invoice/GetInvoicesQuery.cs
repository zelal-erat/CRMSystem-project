using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using MediatR;

namespace CRMSystem.Application.Queries.Invoice;

public record GetInvoicesQuery() : IRequest<Result<IEnumerable<InvoiceDto>>>;
