using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class Sale : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid CashSessionId { get; set; }
    public CashSession CashSession { get; set; } = null!;
    public Guid CashierUserId { get; set; }
    public ApplicationUser CashierUser { get; set; } = null!;
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeDue { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public string? Notes { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
}
