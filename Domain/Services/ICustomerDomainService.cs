using CRMSystem.Application.Common;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Services;

public interface ICustomerDomainService
{
    Task<Result<Customer>> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Result> ValidateCustomerEmailAsync(string email, int? excludeCustomerId = null);
    Task<Result> ValidateCustomerTaxNumberAsync(string taxNumber, int? excludeCustomerId = null);
    Task<Result<Customer>> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request);
    Task<Result> DeleteCustomerAsync(int customerId);
    Task<Result<bool>> CanDeleteCustomerAsync(int customerId);
}

public class CreateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
