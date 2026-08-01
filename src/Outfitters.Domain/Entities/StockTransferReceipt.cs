using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class StockTransferReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid StockTransferId { get; set; }
    public StockTransfer StockTransfer { get; set; } = null!;
    public Guid ReceivedByUserId { get; set; }
    public ApplicationUser ReceivedByUser { get; set; } = null!;
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<StockTransferReceiptItem> Items { get; set; } =
        new List<StockTransferReceiptItem>();
}
