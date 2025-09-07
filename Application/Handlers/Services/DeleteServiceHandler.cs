using CRMSystem.Application.Commands.Service;
using CRMSystem.Application.Common;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;

namespace CRMSystem.Application.Handlers.Services;

public class DeleteServiceHandler : IRequestHandler<DeleteServiceCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceDomainService _serviceDomainService;

    public DeleteServiceHandler(IUnitOfWork unitOfWork, IServiceDomainService serviceDomainService)
    {
        _unitOfWork = unitOfWork;
        _serviceDomainService = serviceDomainService;
    }

    public async Task<Result<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        // Domain Service kullanarak service sil
        var deleteResult = await _serviceDomainService.DeleteServiceAsync(request.Id);
        
        if (!deleteResult.IsSuccess)
            return Result<bool>.Failure(deleteResult.Error);

        return Result<bool>.Success(true);
    }
}


