public class InvoiceDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Şu an string ama enum olarak da tutabilirsin
    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    // Eksik olan alanları ekle
    public DateTime DueDate { get; set; }
    public int ItemCount { get; set; }

    // Yenileme bilgileri
    public bool IsRenewal { get; set; }
    public string RenewalIndicator { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
