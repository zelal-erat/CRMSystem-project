using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CRMDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.TaxNumber == taxNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithInvoicesAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .Include(c => c.Invoices.Where(i => !i.IsDeleted))
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        
        return await _dbSet
            .Where(c => !c.IsDeleted && 
                       (c.FullName.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term) ||
                        c.Phone.Contains(term) ||
                        c.TaxNumber.Contains(term)))
            .ToListAsync();
    }

    public new async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.FullName)
            .ToListAsync();
    }

    // Müşteri bazlı hizmet analizi metodları
    public async Task<IEnumerable<Customer>> GetCustomersWithServiceUsageAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .Include(c => c.Invoices)
                .ThenInclude(i => i.Items)
                    .ThenInclude(item => item.Service)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerWithServiceUsageAsync(int customerId)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.Id == customerId)
            .Include(c => c.Invoices)
                .ThenInclude(i => i.Items)
                    .ThenInclude(item => item.Service)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Customer>> GetCustomersByServiceIdAsync(int serviceId)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && 
                       c.Invoices.Any(i => !i.IsDeleted && 
                                          i.Items.Any(item => !item.IsDeleted && item.ServiceId == serviceId)))
            .Include(c => c.Invoices)
                .ThenInclude(i => i.Items)
                    .ThenInclude(item => item.Service)
            .ToListAsync();
    }

    public async Task<decimal> GetCustomerTotalSpentAsync(int customerId)
    {
        return await _context.InvoiceItems
            .Where(item => !item.IsDeleted && 
                          item.Invoice.CustomerId == customerId && 
                          !item.Invoice.IsDeleted &&
                          item.Invoice.Status == InvoiceStatus.Paid)
            .SumAsync(item => item.TotalAmount);
    }

    public async Task<int> GetCustomerServiceUsageCountAsync(int customerId, int serviceId)
    {
        return await _context.InvoiceItems
            .Where(item => !item.IsDeleted && 
                          item.Invoice.CustomerId == customerId && 
                          !item.Invoice.IsDeleted &&
                          item.ServiceId == serviceId)
            .CountAsync();
    }
}
