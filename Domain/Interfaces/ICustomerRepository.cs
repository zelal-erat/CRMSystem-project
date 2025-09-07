using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.Domain.Interfaces;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByTaxNumberAsync(string taxNumber);
    Task<IEnumerable<Customer>> GetCustomersWithInvoicesAsync();
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    new Task<IEnumerable<Customer>> GetAllAsync();
    
    // Müşteri bazlı hizmet analizi metodları
    Task<IEnumerable<Customer>> GetCustomersWithServiceUsageAsync();
    Task<Customer?> GetCustomerWithServiceUsageAsync(int customerId);
    Task<IEnumerable<Customer>> GetCustomersByServiceIdAsync(int serviceId);
    Task<decimal> GetCustomerTotalSpentAsync(int customerId);
    Task<int> GetCustomerServiceUsageCountAsync(int customerId, int serviceId);
}
