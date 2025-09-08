using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;
using System.Linq.Expressions;

namespace CRMSystem.Application.Handlers.Invoices;

public class SearchInvoicesQueryHandler : IRequestHandler<CRMSystem.Application.Queries.Invoice.SearchInvoicesQuery, Result<PaginatedResultDto<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResultDto<InvoiceDto>>> Handle(CRMSystem.Application.Queries.Invoice.SearchInvoicesQuery request, CancellationToken cancellationToken)
    {
        // Search predicate oluştur - sadece müşteri adıyla
        Expression<Func<CRMSystem.Domain.Entities.Invoice, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            var customerNameLower = request.CustomerName.ToLower();
            predicate = i => i.Customer.FullName.ToLower().Contains(customerNameLower);
        }

        // Paged data al
        var items = await _unitOfWork.Invoices.GetPagedInvoicesAsync(request.PageNumber, request.PageSize, predicate);
        var total = await _unitOfWork.Invoices.CountInvoicesAsync(predicate);

        var dtoItems = _mapper.Map<List<InvoiceDto>>(items);

        var result = PaginatedResultDto<InvoiceDto>.Create(dtoItems, total, request.PageNumber, request.PageSize);

        return Result<PaginatedResultDto<InvoiceDto>>.Success(result);
    }
}
