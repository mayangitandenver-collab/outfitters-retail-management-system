using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class PurchaseOrder : BaseEntity
{
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
