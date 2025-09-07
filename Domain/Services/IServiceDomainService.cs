using CRMSystem.Application.Common;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Services;

public interface IServiceDomainService
{
    Task<Result<Service>> CreateServiceAsync(CreateServiceRequest request);
    Task<Result<Service>> UpdateServiceAsync(int serviceId, UpdateServiceRequest request);
    Task<Result> DeleteServiceAsync(int serviceId);
    Task<Result<bool>> CanDeleteServiceAsync(int serviceId);
    Task<Result> ValidateServiceNameAsync(string name, int? excludeServiceId = null);
}

public class CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
