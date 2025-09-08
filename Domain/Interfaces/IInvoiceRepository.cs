using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using System.Linq.Expressions;

namespace CRMSystem.Domain.Interfaces;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId);
    Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task<IEnumerable<Invoice>> GetUpcomingInvoicesAsync();
    Task<IEnumerable<Invoice>> GetInvoicesWithItemsAsync();
    Task<Invoice?> GetInvoiceWithItemsAsync(int invoiceId);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddInvoiceItemAsync(InvoiceItem item);
    
    // Business Logic için eklenen metodlar
    Task<bool> HasActiveInvoiceAsync(int customerId);
    Task<bool> HasServiceInActiveInvoicesAsync(int serviceId);
    Task<IEnumerable<InvoiceItem>> GetRecurringItemsAsync();
    Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Search metodları - sadece müşteri adıyla
    Task<IEnumerable<Invoice>> SearchInvoicesByCustomerNameAsync(string customerName);
    Task<IEnumerable<Invoice>> GetPagedInvoicesAsync(int pageNumber, int pageSize, Expression<Func<Invoice, bool>>? predicate = null);
    Task<int> CountInvoicesAsync(Expression<Func<Invoice, bool>>? predicate = null);
}
