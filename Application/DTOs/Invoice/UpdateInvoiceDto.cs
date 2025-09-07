namespace CRMSystem.Application.DTOs.Invoice;

public class UpdateInvoiceDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<UpdateInvoiceItemDto> Items { get; set; } = new();
}

public class UpdateInvoiceItemDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string RenewalCycle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal VAT { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
