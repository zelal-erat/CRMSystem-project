using CRMSystem.Domain.Enums;

namespace CRMSystem.Application.DTOs.Invoice;

public class CreateInvoiceItemRequest
{
    public int ServiceId { get; set; }
    public RenewalCycle RenewalCycle { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal VAT { get; set; }
    public DateTime StartDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
