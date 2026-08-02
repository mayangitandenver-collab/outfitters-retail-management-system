using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class ReceiptPrintJob : BaseEntity
{
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public string PrinterName { get; set; } = string.Empty;
    public string PrinterProtocol { get; set; } = "ESC/POS";
    public ReceiptPrintStatus Status { get; set; } = ReceiptPrintStatus.Pending;
    public string ReceiptPayload { get; set; } = string.Empty;
    public int CopyCount { get; set; } = 1;
    public int AttemptCount { get; set; }
    public DateTime? PrintedAtUtc { get; set; }
    public string? LastError { get; set; }
}
