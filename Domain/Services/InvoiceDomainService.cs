using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using CRMSystem.Domain.Interfaces;

namespace CRMSystem.Domain.Services;

public class InvoiceDomainService : IInvoiceDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Invoice>> CreateInvoiceAsync(int customerId, string description, List<CreateInvoiceItemRequest> items)
    {
        // Business Rule: Müşteri kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return Result<Invoice>.Failure("Müşteri bulunamadı.");

        // Business Rule: Müşterinin taslak faturası var mı? (Sadece Draft kontrolü)
        // Not: Bu kural kaldırıldı - müşteriler birden fazla fatura alabilir
        // var hasDraftInvoice = await _unitOfWork.Invoices.HasActiveInvoiceAsync(customerId);
        // if (hasDraftInvoice)
        //     return Result<Invoice>.Failure("Müşterinin zaten taslak faturası var. Önce onu tamamlayın.");

        // Business Rule: En az bir kalem olmalı
        if (items == null || items.Count == 0)
            return Result<Invoice>.Failure("Fatura en az bir kalem içermelidir.");

        // Invoice oluştur
        var invoice = new Invoice
        {
            CustomerId = customerId,
            Description = description,
            Status = InvoiceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        // Invoice Items oluştur
        foreach (var itemRequest in items)
        {
            // Business Rule: Servis kontrolü
            var service = await _unitOfWork.Services.GetByIdAsync(itemRequest.ServiceId);
            if (service == null || service.IsDeleted)
                return Result<Invoice>.Failure($"Servis bulunamadı: {itemRequest.ServiceId}");

            // Business Rule: Fiyat kontrolü
            if (itemRequest.Price <= 0)
                return Result<Invoice>.Failure("Fiyat 0'dan büyük olmalıdır.");

            // Business Rule: Miktar kontrolü
            if (itemRequest.Quantity <= 0)
                return Result<Invoice>.Failure("Miktar 0'dan büyük olmalıdır.");

            var item = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                ServiceId = itemRequest.ServiceId,
                RenewalCycle = itemRequest.RenewalCycle,
                Price = itemRequest.Price,
                Quantity = itemRequest.Quantity,
                VAT = itemRequest.VAT,
                StartDate = itemRequest.StartDate,
                Description = itemRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // DueDate'i otomatik hesapla
            item.CalculateDueDate();

            await _unitOfWork.Invoices.AddInvoiceItemAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();

        return Result<Invoice>.Success(invoice);
    }

    public async Task<Result> ProcessOverdueInvoicesAsync()
    {
        var overdueInvoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();
        
        foreach (var invoice in overdueInvoices)
        {
            // Business Rule: Gecikmiş faturaları otomatik olarak gecikmiş durumuna çevir
            if (invoice.Status == InvoiceStatus.Pending)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> GenerateRecurringInvoicesAsync()
    {
        var recurringItems = await _unitOfWork.Invoices.GetRecurringItemsAsync();
        
        foreach (var item in recurringItems)
        {
            // Business Rule: Yenileme zamanı geldi mi? (Sadece Monthly ve Yearly için)
            if (item.DueDate <= DateTime.UtcNow && 
                item.Invoice.Status == InvoiceStatus.Paid && 
                item.RenewalCycle != RenewalCycle.None)
            {
                // Yeni fatura oluştur
                var newInvoice = new Invoice
                {
                    CustomerId = item.Invoice.CustomerId,
                    Description = $"🔄 YENİLEME - {item.Service.Name} (Orijinal Fatura: #{item.Invoice.Id})",
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Invoices.AddAsync(newInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Yeni item oluştur
                var newItem = new InvoiceItem
                {
                    InvoiceId = newInvoice.Id,
                    ServiceId = item.ServiceId,
                    RenewalCycle = item.RenewalCycle,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    VAT = item.VAT,
                    StartDate = DateTime.UtcNow,
                    Description = $"🔄 YENİLEME - {item.Description}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                newItem.CalculateDueDate();
                await _unitOfWork.Invoices.AddInvoiceItemAsync(newItem);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<decimal>> CalculateInvoiceTotalAsync(Invoice invoice)
    {
        if (invoice == null)
            return Result<decimal>.Failure("Fatura bulunamadı.");

        var total = invoice.Items
            .Where(i => !i.IsDeleted)
            .Sum(i => i.TotalAmount);

        return Result<decimal>.Success(total);
    }

    public async Task<Result> ValidateInvoiceCreationAsync(int customerId, List<CreateInvoiceItemRequest> items)
    {
        // Müşteri kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return Result.Failure("Müşteri bulunamadı.");

        // Kalem kontrolü
        if (items == null || items.Count == 0)
            return Result.Failure("En az bir fatura kalemi gerekli.");

        // Servis kontrolü
        foreach (var item in items)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(item.ServiceId);
            if (service == null || service.IsDeleted)
                return Result.Failure($"Servis bulunamadı: {item.ServiceId}");
        }

        return Result.Success();
    }

    public async Task<Result> MarkInvoiceAsPaidAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null || invoice.IsDeleted)
            return Result.Failure("Fatura bulunamadı.");

        // Business Rule: Ödenmiş ve iptal edilmiş faturalar tekrar ödenemez
        if (invoice.Status == InvoiceStatus.Paid)
            return Result.Failure("Bu fatura zaten ödenmiş.");
            
        if (invoice.Status == InvoiceStatus.Cancelled)
            return Result.Failure("İptal edilmiş faturalar ödenemez.");

        invoice.Status = InvoiceStatus.Paid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CancelInvoiceAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null || invoice.IsDeleted)
            return Result.Failure("Fatura bulunamadı.");

        // Business Rule: Ödenmiş faturalar iptal edilemez
        if (invoice.Status == InvoiceStatus.Paid)
            return Result.Failure("Ödenmiş faturalar iptal edilemez.");

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Invoice>> UpdateInvoiceAsync(int invoiceId, int customerId, string description, string status, List<CreateInvoiceItemRequest> items)
    {
        var existing = await _unitOfWork.Invoices.GetInvoiceWithItemsAsync(invoiceId);
        if (existing == null || existing.IsDeleted)
            return Result<Invoice>.Failure("Fatura bulunamadı.");

        // Business Rule: Müşteri kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return Result<Invoice>.Failure("Müşteri bulunamadı.");

        // Business Rule: Status kontrolü
        if (!Enum.TryParse<InvoiceStatus>(status, out var invoiceStatus))
            return Result<Invoice>.Failure("Geçersiz fatura durumu.");

        // Business Rule: En az bir kalem olmalı
        if (items == null || items.Count == 0)
            return Result<Invoice>.Failure("Fatura en az bir kalem içermelidir.");

        // Invoice güncelle
        existing.CustomerId = customerId;
        existing.Description = description;
        existing.Status = invoiceStatus;
        existing.UpdatedAt = DateTime.UtcNow;

        // Business Rule: Tarih değişikliği sonrası durum kontrolü
        await CheckAndUpdateInvoiceStatusAsync(existing);

        // Mevcut items'ları sil (soft delete)
        foreach (var item in existing.Items)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        // Yeni items'ları ekle
        foreach (var itemRequest in items)
        {
            // Business Rule: Servis kontrolü
            var service = await _unitOfWork.Services.GetByIdAsync(itemRequest.ServiceId);
            if (service == null || service.IsDeleted)
                return Result<Invoice>.Failure($"Servis bulunamadı: {itemRequest.ServiceId}");

            // Business Rule: Fiyat kontrolü
            if (itemRequest.Price <= 0)
                return Result<Invoice>.Failure("Fiyat 0'dan büyük olmalıdır.");

            // Business Rule: Miktar kontrolü
            if (itemRequest.Quantity <= 0)
                return Result<Invoice>.Failure("Miktar 0'dan büyük olmalıdır.");

            var item = new InvoiceItem
            {
                InvoiceId = existing.Id,
                ServiceId = itemRequest.ServiceId,
                RenewalCycle = itemRequest.RenewalCycle,
                Price = itemRequest.Price,
                Quantity = itemRequest.Quantity,
                VAT = itemRequest.VAT,
                StartDate = itemRequest.StartDate,
                Description = itemRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // DueDate'i otomatik hesapla
            item.CalculateDueDate();

            await _unitOfWork.Invoices.AddInvoiceItemAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();

        // Business Rule: Yeni kalemler eklendikten sonra durum kontrolü
        await CheckAndUpdateInvoiceStatusAsync(existing);

        return Result<Invoice>.Success(existing);
    }

    public async Task<Result> DeleteInvoiceAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null || invoice.IsDeleted)
            return Result.Failure("Fatura bulunamadı.");

        // Business Rule: Ödenmiş faturalar silinemez
        if (invoice.Status == InvoiceStatus.Paid)
            return Result.Failure("Ödenmiş faturalar silinemez.");

        // Soft delete
        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    // Yeni business rule metodları
    public async Task<Result<decimal>> GetServicePriceAsync(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null || service.IsDeleted)
            return Result<decimal>.Failure("Servis bulunamadı.");

        return Result<decimal>.Success(service.Price);
    }

    public async Task<Result> AutoUpdateOverdueInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var overdueInvoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();
        
        foreach (var invoice in overdueInvoices)
        {
            // Business Rule: Vade tarihi geçmiş ama "Paid" olmayan faturalar otomatik "Overdue" yapılır
            if (invoice.Status != InvoiceStatus.Paid && 
                invoice.Items.Any(i => !i.IsDeleted && 
                                      i.DueDate != DateTime.MaxValue && 
                                      i.DueDate < today))
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    // Yeni metod: Fatura durumunu otomatik kontrol et ve güncelle
    public async Task CheckAndUpdateInvoiceStatusAsync(Invoice invoice)
    {
        var today = DateTime.UtcNow.Date;
        
        // Business Rule: Ödenmiş faturaların durumu değiştirilemez
        if (invoice.Status == InvoiceStatus.Paid)
            return;

        // Business Rule: Vade tarihi geçmiş faturalar otomatik "Overdue" olur
        var hasOverdueItems = invoice.Items.Any(i => !i.IsDeleted && 
                                                   i.DueDate != DateTime.MaxValue && 
                                                   i.DueDate < today);
        
        if (hasOverdueItems && invoice.Status != InvoiceStatus.Paid)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;
        }
        // Business Rule: Vade tarihi henüz gelmemiş faturalar "Pending" olur
        else if (!hasOverdueItems && invoice.Status == InvoiceStatus.Overdue)
        {
            invoice.Status = InvoiceStatus.Pending;
            invoice.UpdatedAt = DateTime.UtcNow;
        }
    }
}
