using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class AccountsPayable : BaseEntity
{
    public string PayableNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public DateTime InvoiceDateUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public PayableStatus Status { get; set; } = PayableStatus.Open;
    public string? SupplierInvoiceNumber { get; set; }
}
