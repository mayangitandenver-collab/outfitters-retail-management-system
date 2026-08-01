using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class StockTransferReceiptItem : BaseEntity
{
    public Guid StockTransferReceiptId { get; set; }
    public StockTransferReceipt StockTransferReceipt { get; set; } = null!;
    public Guid StockTransferItemId { get; set; }
    public StockTransferItem StockTransferItem { get; set; } = null!;
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public decimal QuantityReceived { get; set; }
    public decimal QuantityDamaged { get; set; }
}
