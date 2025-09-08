using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.DTOs.Common;
using MediatR;

namespace CRMSystem.Application.Queries.Invoice;

public record SearchInvoicesQuery(
    string? CustomerName,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedResultDto<InvoiceDto>>>;
