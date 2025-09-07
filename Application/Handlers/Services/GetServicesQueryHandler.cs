using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Application.Queries.Service;
using CRMSystem.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Services;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, Result<IEnumerable<ServiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetServicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Services.GetActiveServicesAsync();
        var dtos = _mapper.Map<IEnumerable<ServiceDto>>(items);
        return Result<IEnumerable<ServiceDto>>.Success(dtos);
    }
}


