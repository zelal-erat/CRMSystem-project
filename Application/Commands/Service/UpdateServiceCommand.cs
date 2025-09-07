using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Service;
using MediatR;

namespace CRMSystem.Application.Commands.Service;

public record UpdateServiceCommand(UpdateServiceDto Service) : IRequest<Result<ServiceDto>>;


