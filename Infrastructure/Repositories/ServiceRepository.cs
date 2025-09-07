using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Infrastructure.Repositories;

public class ServiceRepository : GenericRepository<Service>, IServiceRepository
{
    public ServiceRepository(CRMDbContext context) : base(context)
    {
    }

    public async Task<Service?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveServicesAsync()
    {
        return await _dbSet
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> SearchServicesAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        
        return await _dbSet
            .Where(s => !s.IsDeleted && 
                       (s.Name.ToLower().Contains(term) ||
                        s.Description.ToLower().Contains(term)))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    // Business Logic için eklenen metodlar
    public async Task<bool> HasServiceInActiveInvoicesAsync(int serviceId)
    {
        return await _context.InvoiceItems
            .AnyAsync(i => !i.IsDeleted && 
                         i.ServiceId == serviceId && 
                         !i.Invoice.IsDeleted &&
                         i.Invoice.Status == InvoiceStatus.Pending);
    }
}
