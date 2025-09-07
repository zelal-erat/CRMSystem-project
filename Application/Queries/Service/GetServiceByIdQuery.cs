using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using MediatR;

namespace CRMSystem.Application.Queries.Service;

public record GetServiceByIdQuery(int Id) : IRequest<Result<ServiceDto>>;


