using CRMSystem.Application.DTOs.Service;
using MediatR;
using CRMSystem.Application.Common;

namespace CRMSystem.Application.Commands.Service;

public record CreateServiceCommand(CreateServiceDto Service) : IRequest<Result<ServiceDto>>;
