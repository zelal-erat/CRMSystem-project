using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

public class InvoiceItem : BaseEntity
{
    [Required]
    public int InvoiceId { get; set; }

     [Required]
    public RenewalCycle RenewalCycle { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(5,2)")]
    public decimal VAT { get; set; } = 0;


    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime DueDate { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public decimal TotalAmount => (Price * Quantity) * (1 + (VAT / 100));

    // Navigation
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}