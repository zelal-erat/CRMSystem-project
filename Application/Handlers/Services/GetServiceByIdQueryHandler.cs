using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Application.Queries.Service;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Services;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetServiceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Services.GetByIdAsync(request.Id);
        if (entity is null || entity.IsDeleted)
            return Result<ServiceDto>.Failure("Servis bulunamadı.");
        var dto = _mapper.Map<ServiceDto>(entity);
        return Result<ServiceDto>.Success(dto);
    }
}


