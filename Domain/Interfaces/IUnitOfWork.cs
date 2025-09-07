using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IInvoiceRepository Invoices { get; }
    IServiceRepository Services { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
