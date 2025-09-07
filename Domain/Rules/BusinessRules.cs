using CRMSystem.Application.Common;
using CRMSystem.Domain.Interfaces;

namespace CRMSystem.Domain.Rules;

public class CustomerEmailMustBeUniqueRule : IBusinessRule
{
    private readonly string _email;
    private readonly int? _customerId;
    private readonly IUnitOfWork _unitOfWork;

    public string ErrorMessage => "Bu e-posta adresi zaten kullanılıyor.";

    public CustomerEmailMustBeUniqueRule(string email, int? customerId, IUnitOfWork unitOfWork)
    {
        _email = email;
        _customerId = customerId;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsValidAsync()
    {
        if (string.IsNullOrWhiteSpace(_email))
            return false;

        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(_email.Trim().ToLower());
        return existingCustomer == null || existingCustomer.Id == _customerId;
    }
}

public class CustomerTaxNumberMustBeUniqueRule : IBusinessRule
{
    private readonly string _taxNumber;
    private readonly int? _customerId;
    private readonly IUnitOfWork _unitOfWork;

    public string ErrorMessage => "Bu vergi numarası zaten kullanılıyor.";

    public CustomerTaxNumberMustBeUniqueRule(string taxNumber, int? customerId, IUnitOfWork unitOfWork)
    {
        _taxNumber = taxNumber;
        _customerId = customerId;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsValidAsync()
    {
        if (string.IsNullOrWhiteSpace(_taxNumber))
            return true; // Vergi numarası opsiyonel

        var existingCustomer = await _unitOfWork.Customers.GetByTaxNumberAsync(_taxNumber.Trim());
        return existingCustomer == null || existingCustomer.Id == _customerId;
    }
}

public class InvoiceMustHaveAtLeastOneItemRule : IBusinessRule
{
    private readonly int _itemCount;

    public string ErrorMessage => "Fatura en az bir kalem içermelidir.";

    public InvoiceMustHaveAtLeastOneItemRule(int itemCount)
    {
        _itemCount = itemCount;
    }

    public async Task<bool> IsValidAsync()
    {
        return _itemCount > 0;
    }
}

public class CustomerMustExistRule : IBusinessRule
{
    private readonly int _customerId;
    private readonly IUnitOfWork _unitOfWork;

    public string ErrorMessage => "Müşteri bulunamadı.";

    public CustomerMustExistRule(int customerId, IUnitOfWork unitOfWork)
    {
        _customerId = customerId;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsValidAsync()
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(_customerId);
        return customer != null && !customer.IsDeleted;
    }
}

public class ServiceMustExistRule : IBusinessRule
{
    private readonly int _serviceId;
    private readonly IUnitOfWork _unitOfWork;

    public string ErrorMessage => $"Servis bulunamadı: {_serviceId}";

    public ServiceMustExistRule(int serviceId, IUnitOfWork unitOfWork)
    {
        _serviceId = serviceId;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsValidAsync()
    {
        var service = await _unitOfWork.Services.GetByIdAsync(_serviceId);
        return service != null && !service.IsDeleted;
    }
}

public class InvoicePriceMustBePositiveRule : IBusinessRule
{
    private readonly decimal _price;

    public string ErrorMessage => "Fiyat 0'dan büyük olmalıdır.";

    public InvoicePriceMustBePositiveRule(decimal price)
    {
        _price = price;
    }

    public async Task<bool> IsValidAsync()
    {
        return _price > 0;
    }
}

public class InvoiceQuantityMustBePositiveRule : IBusinessRule
{
    private readonly int _quantity;

    public string ErrorMessage => "Miktar 0'dan büyük olmalıdır.";

    public InvoiceQuantityMustBePositiveRule(int quantity)
    {
        _quantity = quantity;
    }

    public async Task<bool> IsValidAsync()
    {
        return _quantity > 0;
    }
}

public class InvoiceCanBePaidRule : IBusinessRule
{
    private readonly string _status;

    public string ErrorMessage => "Sadece bekleyen faturalar ödenebilir.";

    public InvoiceCanBePaidRule(string status)
    {
        _status = status;
    }

    public async Task<bool> IsValidAsync()
    {
        return _status == "Pending";
    }
}

public class CustomerCanBeDeletedRule : IBusinessRule
{
    private readonly int _customerId;
    private readonly IUnitOfWork _unitOfWork;

    public string ErrorMessage => "Aktif faturası olan müşteri silinemez.";

    public CustomerCanBeDeletedRule(int customerId, IUnitOfWork unitOfWork)
    {
        _customerId = customerId;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsValidAsync()
    {
        var hasActiveInvoices = await _unitOfWork.Invoices.HasActiveInvoiceAsync(_customerId);
        return !hasActiveInvoices;
    }
}
