using CRMSystem.Application.Common;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;

namespace CRMSystem.Domain.Services;

public class CustomerDomainService : ICustomerDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Customer>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // Business Rule: E-posta benzersizlik kontrolü
        var emailValidation = await ValidateCustomerEmailAsync(request.Email);
        if (!emailValidation.IsSuccess)
            return Result<Customer>.Failure(emailValidation.Error);

        // Business Rule: Vergi numarası benzersizlik kontrolü (boş değilse)
        if (!string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            var taxValidation = await ValidateCustomerTaxNumberAsync(request.TaxNumber);
            if (!taxValidation.IsSuccess)
                return Result<Customer>.Failure(taxValidation.Error);
        }

        // Business Rule: Ad Soyad kontrolü
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result<Customer>.Failure("Ad Soyad zorunludur.");

        // Business Rule: E-posta format kontrolü
        if (!IsValidEmail(request.Email))
            return Result<Customer>.Failure("Geçerli bir e-posta adresi giriniz.");

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone?.Trim() ?? string.Empty,
            TaxOffice = request.TaxOffice?.Trim() ?? string.Empty,
            TaxNumber = request.TaxNumber?.Trim() ?? string.Empty,
            Address = request.Address?.Trim() ?? string.Empty,
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return Result<Customer>.Success(customer);
    }

    public async Task<Result> ValidateCustomerEmailAsync(string email, int? excludeCustomerId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("E-posta adresi zorunludur.");

        if (!IsValidEmail(email))
            return Result.Failure("Geçerli bir e-posta adresi giriniz.");

        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(email.Trim().ToLower());
        
        if (existingCustomer != null && existingCustomer.Id != excludeCustomerId)
            return Result.Failure("Bu e-posta adresi zaten kullanılıyor.");

        return Result.Success();
    }

    public async Task<Result> ValidateCustomerTaxNumberAsync(string taxNumber, int? excludeCustomerId = null)
    {
        if (string.IsNullOrWhiteSpace(taxNumber))
            return Result.Success(); // Vergi numarası opsiyonel

        // Business Rule: Vergi numarası format kontrolü
        if (!IsValidTaxNumber(taxNumber))
            return Result.Failure("Geçerli bir vergi numarası giriniz.");

        var existingCustomer = await _unitOfWork.Customers.GetByTaxNumberAsync(taxNumber.Trim());
        
        if (existingCustomer != null && existingCustomer.Id != excludeCustomerId)
            return Result.Failure("Bu vergi numarası zaten kullanılıyor.");

        return Result.Success();
    }

    public async Task<Result<Customer>> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request)
    {
        var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (existingCustomer == null || existingCustomer.IsDeleted)
            return Result<Customer>.Failure("Müşteri bulunamadı.");

        // Business Rule: E-posta benzersizlik kontrolü (kendi e-postası hariç)
        var emailValidation = await ValidateCustomerEmailAsync(request.Email, customerId);
        if (!emailValidation.IsSuccess)
            return Result<Customer>.Failure(emailValidation.Error);

        // Business Rule: Vergi numarası benzersizlik kontrolü (kendi vergi numarası hariç)
        if (!string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            var taxValidation = await ValidateCustomerTaxNumberAsync(request.TaxNumber, customerId);
            if (!taxValidation.IsSuccess)
                return Result<Customer>.Failure(taxValidation.Error);
        }

        // Business Rule: Ad Soyad kontrolü
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result<Customer>.Failure("Ad Soyad zorunludur.");

        // Müşteri bilgilerini güncelle
        existingCustomer.FullName = request.FullName.Trim();
        existingCustomer.Email = request.Email.Trim().ToLower();
        existingCustomer.Phone = request.Phone?.Trim() ?? string.Empty;
        existingCustomer.TaxOffice = request.TaxOffice?.Trim() ?? string.Empty;
        existingCustomer.TaxNumber = request.TaxNumber?.Trim() ?? string.Empty;
        existingCustomer.Address = request.Address?.Trim() ?? string.Empty;
        existingCustomer.Description = request.Description?.Trim() ?? string.Empty;
        existingCustomer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return Result<Customer>.Success(existingCustomer);
    }

    public async Task<Result> DeleteCustomerAsync(int customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return Result.Failure("Müşteri bulunamadı.");

        // Business Rule: Müşteri silinebilir mi kontrolü
        var canDelete = await CanDeleteCustomerAsync(customerId);
        if (!canDelete.IsSuccess)
            return Result.Failure(canDelete.Error);

        // Soft delete
        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<bool>> CanDeleteCustomerAsync(int customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return Result<bool>.Failure("Müşteri bulunamadı.");

        // Business Rule: Aktif faturası olan müşteri silinemez
        var hasActiveInvoices = await _unitOfWork.Invoices.HasActiveInvoiceAsync(customerId);
        if (hasActiveInvoices)
            return Result<bool>.Failure("Aktif faturası olan müşteri silinemez.");

        return Result<bool>.Success(true);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidTaxNumber(string taxNumber)
    {
        // Basit vergi numarası format kontrolü (10 haneli sayı)
        return taxNumber.Length == 10 && taxNumber.All(char.IsDigit);
    }
}
