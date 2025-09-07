using CRMSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRMSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CRMDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private ICustomerRepository? _customers;
    private IInvoiceRepository? _invoices;
    private IServiceRepository? _services;

    public UnitOfWork(CRMDbContext context)
    {
        _context = context;
    }

    public ICustomerRepository Customers => 
        _customers ??= new CustomerRepository(_context);

    public IInvoiceRepository Invoices => 
        _invoices ??= new InvoiceRepository(_context);

    public IServiceRepository Services => 
        _services ??= new ServiceRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
