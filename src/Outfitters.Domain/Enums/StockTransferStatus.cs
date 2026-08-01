namespace Outfitters.Domain.Enums;

public enum StockTransferStatus
{
    Draft = 1,
    Submitted = 2,
    InTransit = 3,
    PartiallyReceived = 4,
    FullyReceived = 5,
    Cancelled = 6
}
