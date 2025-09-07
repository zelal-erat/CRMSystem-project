using CRMSystem.Application.Common;
using MediatR;

namespace CRMSystem.Application.Commands.Service;

public record DeleteServiceCommand(int Id) : IRequest<Result<bool>>;


