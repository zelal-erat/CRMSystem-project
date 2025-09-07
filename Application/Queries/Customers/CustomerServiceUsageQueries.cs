using MediatR;
using CRMSystem.Application.DTOs.Customer;

namespace CRMSystem.Application.Queries.Customers;

public class GetCustomerServiceUsageQuery : IRequest<CRMSystem.Application.Common.Result<CustomerServiceUsageDto?>>
{
    public int CustomerId { get; set; }
}

public class GetCustomerServiceAnalysisQuery : IRequest<CRMSystem.Application.Common.Result<CustomerServiceAnalysisDto>>
{
    public bool IncludeInactiveCustomers { get; set; } = false;
}

public class GetServiceUsageByCustomerQuery : IRequest<CRMSystem.Application.Common.Result<IEnumerable<CustomerServiceUsageDto>>>
{
    public int? ServiceId { get; set; }
}
