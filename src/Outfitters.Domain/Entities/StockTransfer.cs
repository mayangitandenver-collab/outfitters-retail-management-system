using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public Guid SourceStoreId { get; set; }
    public Store SourceStore { get; set; } = null!;
    public Guid DestinationStoreId { get; set; }
    public Store DestinationStore { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public Guid? DispatchedByUserId { get; set; }
    public ApplicationUser? DispatchedByUser { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public ApplicationUser? ReceivedByUser { get; set; }
    public DateTime TransferDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DispatchedAtUtc { get; set; }
    public DateTime? ReceivedAtUtc { get; set; }
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;
    public string? Notes { get; set; }
    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
    public ICollection<StockTransferReceipt> Receipts { get; set; } = new List<StockTransferReceipt>();
}
