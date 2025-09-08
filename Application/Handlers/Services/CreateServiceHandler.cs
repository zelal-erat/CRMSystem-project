using CRMSystem.Application.Commands.Service;
using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Services;

public class CreateServiceHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IServiceDomainService _serviceDomainService;

    public CreateServiceHandler(IUnitOfWork unitOfWork, IMapper mapper, IServiceDomainService serviceDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _serviceDomainService = serviceDomainService;
    }       

    public async Task<Result<ServiceDto>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        // AutoMapper ile DTO'yu Domain Request'e çevir
        var createRequest = _mapper.Map<CreateServiceRequest>(request.Service);

        var serviceResult = await _serviceDomainService.CreateServiceAsync(createRequest);
        
        if (!serviceResult.IsSuccess)
            return Result<ServiceDto>.Failure(serviceResult.Error);
        
        return Result<ServiceDto>.Success(_mapper.Map<ServiceDto>(serviceResult.Data));
    }
}
 
