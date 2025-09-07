using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Services;

public interface IInvoiceDomainService
{
    Task<Result<Invoice>> CreateInvoiceAsync(int customerId, string description, List<CreateInvoiceItemRequest> items);
    Task<Result<Invoice>> UpdateInvoiceAsync(int invoiceId, int customerId, string description, string status, List<CreateInvoiceItemRequest> items);
    Task<Result> ProcessOverdueInvoicesAsync();
    Task<Result> GenerateRecurringInvoicesAsync();
    Task<Result<decimal>> CalculateInvoiceTotalAsync(Invoice invoice);
    Task<Result> ValidateInvoiceCreationAsync(int customerId, List<CreateInvoiceItemRequest> items);
    Task<Result> MarkInvoiceAsPaidAsync(int invoiceId);
    Task<Result> CancelInvoiceAsync(int invoiceId);
    Task<Result> DeleteInvoiceAsync(int invoiceId);
    
    // Yeni business rule metodları
    Task<Result<decimal>> GetServicePriceAsync(int serviceId);
    Task<Result> AutoUpdateOverdueInvoicesAsync();
    Task CheckAndUpdateInvoiceStatusAsync(Invoice invoice);
}
