namespace CRMSystem.Application.DTOs.Invoice;

public class CreateInvoiceDto
{
    public int CustomerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<CreateInvoiceItemDto> Items { get; set; } = new();
}

public class CreateInvoiceItemDto
{
    public int ServiceId { get; set; }
    public string RenewalCycle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal VAT { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
