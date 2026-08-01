using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class GoodsReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid ReceivedByUserId { get; set; }
    public ApplicationUser ReceivedByUser { get; set; } = null!;
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public string? SupplierInvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}
