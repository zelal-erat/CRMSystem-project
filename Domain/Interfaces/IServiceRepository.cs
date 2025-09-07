using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Interfaces;

public interface IServiceRepository : IGenericRepository<Service>
{
    Task<Service?> GetByNameAsync(string name);
    Task<IEnumerable<Service>> GetActiveServicesAsync();
    Task<IEnumerable<Service>> SearchServicesAsync(string searchTerm);
    
    // Business Logic için eklenen metodlar
    Task<bool> HasServiceInActiveInvoicesAsync(int serviceId);
}
