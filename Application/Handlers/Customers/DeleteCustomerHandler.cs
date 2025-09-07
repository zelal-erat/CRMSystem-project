using CRMSystem.Application.Commands.Customers;
using CRMSystem.Application.Common;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;

namespace CRMSystem.Application.Handlers.Customers;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerDomainService _customerDomainService;

    public DeleteCustomerHandler(IUnitOfWork unitOfWork, ICustomerDomainService customerDomainService)
    {       
        _unitOfWork = unitOfWork;
        _customerDomainService = customerDomainService;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        // Domain Service kullanarak customer sil
        var deleteResult = await _customerDomainService.DeleteCustomerAsync(request.Id);
        
        if (!deleteResult.IsSuccess)
            return Result<bool>.Failure(deleteResult.Error);

        return Result<bool>.Success(true);
    }
}

