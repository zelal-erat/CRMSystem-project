using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Dashboard;
using MediatR;

namespace CRMSystem.Application.Queries.Dashboard;

public record GetDashboardQuery() : IRequest<Result<DashboardDto>>;
