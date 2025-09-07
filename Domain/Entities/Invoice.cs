using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRMSystem.Domain.Enums;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Domain.Entities;

public class Invoice : BaseEntity
{
    [Required]
    public int CustomerId { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;


    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    [Required]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    // Toplam tutar hesaplaması - optimize edilmiş
    [NotMapped]
    public decimal TotalAmount => Items.Where(i => !i.IsDeleted).Sum(i => i.TotalAmount);

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}