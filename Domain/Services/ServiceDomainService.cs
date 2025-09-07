using CRMSystem.Application.Common;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;

namespace CRMSystem.Domain.Services;

public class ServiceDomainService : IServiceDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Service>> CreateServiceAsync(CreateServiceRequest request)
    {
        // Business Rule: Servis adı benzersizlik kontrolü
        var nameValidation = await ValidateServiceNameAsync(request.Name);
        if (!nameValidation.IsSuccess)
            return Result<Service>.Failure(nameValidation.Error);

        // Business Rule: Ad kontrolü
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Service>.Failure("Servis adı zorunludur.");

        // Business Rule: Fiyat kontrolü
        if (request.Price < 0)
            return Result<Service>.Failure("Fiyat negatif olamaz.");

        var service = new Service
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Services.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();

        return Result<Service>.Success(service);
    }

    public async Task<Result<Service>> UpdateServiceAsync(int serviceId, UpdateServiceRequest request)
    {
        var existingService = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (existingService == null || existingService.IsDeleted)
            return Result<Service>.Failure("Servis bulunamadı.");

        // Business Rule: Servis adı benzersizlik kontrolü (kendi adı hariç)
        var nameValidation = await ValidateServiceNameAsync(request.Name, serviceId);
        if (!nameValidation.IsSuccess)
            return Result<Service>.Failure(nameValidation.Error);

        // Business Rule: Ad kontrolü
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Service>.Failure("Servis adı zorunludur.");

        // Business Rule: Fiyat kontrolü
        if (request.Price < 0)
            return Result<Service>.Failure("Fiyat negatif olamaz.");

        // Servis bilgilerini güncelle
        existingService.Name = request.Name.Trim();
        existingService.Price = request.Price;
        existingService.Description = request.Description?.Trim() ?? string.Empty;
        existingService.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return Result<Service>.Success(existingService);
    }

    public async Task<Result> DeleteServiceAsync(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null || service.IsDeleted)
            return Result.Failure("Servis bulunamadı.");

        // Business Rule: Servis silinebilir mi kontrolü
        var canDelete = await CanDeleteServiceAsync(serviceId);
        if (!canDelete.IsSuccess)
            return Result.Failure(canDelete.Error);

        // Soft delete
        service.IsDeleted = true;
        service.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<bool>> CanDeleteServiceAsync(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null || service.IsDeleted)
            return Result<bool>.Failure("Servis bulunamadı.");

        // Business Rule: Aktif fatura kaleminde kullanılan servis silinemez
        var hasActiveInvoiceItems = await _unitOfWork.Invoices.HasServiceInActiveInvoicesAsync(serviceId);
        if (hasActiveInvoiceItems)
            return Result<bool>.Failure("Aktif faturalarda kullanılan servis silinemez.");

        return Result<bool>.Success(true);
    }

    public async Task<Result> ValidateServiceNameAsync(string name, int? excludeServiceId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Servis adı zorunludur.");

        // Business Rule: Servis adı benzersizlik kontrolü
        var existingService = await _unitOfWork.Services.GetByNameAsync(name.Trim());
        
        if (existingService != null && existingService.Id != excludeServiceId)
            return Result.Failure("Bu servis adı zaten kullanılıyor.");

        return Result.Success();
    }
}
