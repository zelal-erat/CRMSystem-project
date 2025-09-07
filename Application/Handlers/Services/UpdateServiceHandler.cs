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
        // Domain Service kullanarak service güncelle
        var updateRequest = new UpdateServiceRequest
        {
            Name = request.Service.Name,
            Price = request.Service.Price,
            Description = request.Service.Description
        };

        var serviceResult = await _serviceDomainService.UpdateServiceAsync(request.Service.Id, updateRequest);

        if (!serviceResult.IsSuccess)
            return Result<ServiceDto>.Failure(serviceResult.Error);

        var dto = _mapper.Map<ServiceDto>(serviceResult.Data);
        return Result<ServiceDto>.Success(dto);
    }
}


