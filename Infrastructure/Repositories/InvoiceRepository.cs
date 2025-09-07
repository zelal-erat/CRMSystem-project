using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using CRMSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Infrastructure.Repositories;

public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(CRMDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && i.CustomerId == customerId)
            .Include(i => i.Items.Where(item => !item.IsDeleted))
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status)
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && i.Status == status)
            .Include(i => i.Items.Where(item => !item.IsDeleted))
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date; // ✅ PostgreSQL için UTC kullan
        
        return await _dbSet
            .Where(i => !i.IsDeleted && 
                       (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue) && // ✅ Sadece aktif faturalar
                       i.Items.Any(item => !item.IsDeleted && 
                                         item.DueDate != DateTime.MaxValue && // ✅ Tek seferlik hizmetler hariç
                                         item.DueDate < today)) // ✅ Vade geçmiş
            .Include(i => i.Items.Where(item => !item.IsDeleted))
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetUpcomingInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date; // ✅ PostgreSQL için UTC kullan
        var upcomingDate = today.AddDays(7); // ✅ Dashboard ile uyumlu: 7 gün
        
        return await _dbSet
            .Where(i => !i.IsDeleted && 
                       (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue) && // ✅ Sadece aktif faturalar
                       i.Items.Any(item => !item.IsDeleted && 
                                         item.DueDate != DateTime.MaxValue && // ✅ Tek seferlik hizmetler hariç
                                         item.DueDate >= today && 
                                         item.DueDate <= upcomingDate)) // ✅ 7 gün içinde
            .Include(i => i.Items.Where(item => !item.IsDeleted))
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesWithItemsAsync()
    {
        return await _dbSet
            .Where(i => !i.IsDeleted)
            .Include(i => i.Items.Where(item => !item.IsDeleted))
                .ThenInclude(item => item.Service)
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceWithItemsAsync(int invoiceId)
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && i.Id == invoiceId)
            .Include(i => i.Items.Where(item => !item.IsDeleted))
                .ThenInclude(item => item.Service)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid)
            .SelectMany(i => i.Items.Where(item => !item.IsDeleted))
            .SumAsync(item => (item.Price * item.Quantity) * (1 + (item.VAT / 100)));
    }

    public async Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && 
                       i.Status == InvoiceStatus.Paid &&
                       i.CreatedAt >= startDate && 
                       i.CreatedAt <= endDate)
            .SelectMany(i => i.Items.Where(item => !item.IsDeleted))
            .SumAsync(item => (item.Price * item.Quantity) * (1 + (item.VAT / 100)));
    }

    public async Task AddInvoiceItemAsync(InvoiceItem item)
    {
        await _context.InvoiceItems.AddAsync(item);
    }

    // Business Logic için eklenen metodlar
    public async Task<bool> HasActiveInvoiceAsync(int customerId)
    {
        return await _dbSet
            .AnyAsync(i => !i.IsDeleted && 
                          i.CustomerId == customerId && 
                          i.Status == InvoiceStatus.Pending); // Sadece Pending kontrolü
    }

    public async Task<bool> HasServiceInActiveInvoicesAsync(int serviceId)
    {
        return await _context.InvoiceItems
            .AnyAsync(i => !i.IsDeleted && 
                          i.ServiceId == serviceId &&
                          !i.Invoice.IsDeleted &&
                          i.Invoice.Status == InvoiceStatus.Pending);
    }

    public async Task<IEnumerable<InvoiceItem>> GetRecurringItemsAsync()
    {
        return await _context.InvoiceItems
            .Where(i => !i.IsDeleted && 
                       i.Invoice.Status == InvoiceStatus.Paid &&
                       i.DueDate != DateTime.MaxValue && // ✅ Tek seferlik hizmetler hariç
                       i.DueDate <= DateTime.UtcNow)
            .Include(i => i.Invoice)
            .Include(i => i.Service)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(i => !i.IsDeleted && 
                       i.CreatedAt >= startDate && 
                       i.CreatedAt <= endDate)
            .Include(i => i.Items.Where(item => !item.IsDeleted))
            .Include(i => i.Customer)
            .ToListAsync();
    }
}
