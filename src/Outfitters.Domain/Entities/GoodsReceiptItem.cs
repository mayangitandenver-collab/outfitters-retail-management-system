using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class GoodsReceiptItem : BaseEntity
{
    public Guid GoodsReceiptId { get; set; }
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public Guid PurchaseOrderItemId { get; set; }
    public PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}
