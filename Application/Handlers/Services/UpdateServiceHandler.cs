using CRMSystem.Application.Commands.Service;
using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Services;

public class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IServiceDomainService _serviceDomainService;

    public UpdateServiceHandler(IUnitOfWork unitOfWork, IMapper mapper, IServiceDomainService serviceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _serviceDomainService = serviceDomainService;
    }

    public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        // AutoMapper ile DTO'yu Domain Request'e çevir
        var updateRequest = _mapper.Map<UpdateServiceRequest>(request.Service);

        var serviceResult = await _serviceDomainService.UpdateServiceAsync(request.Service.Id, updateRequest);

        if (!serviceResult.IsSuccess)
            return Result<ServiceDto>.Failure(serviceResult.Error);

        return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(serviceResult.Data));
    }
}


