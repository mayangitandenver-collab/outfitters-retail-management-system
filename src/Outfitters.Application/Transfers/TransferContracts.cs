namespace Outfitters.Application.Transfers;

public sealed record StockTransferItemRequest(
    Guid ProductVariantId,
    decimal RequestedQuantity);

public sealed record CreateStockTransferRequest(
    Guid SourceStoreId,
    Guid DestinationStoreId,
    string? Notes,
    IReadOnlyCollection<StockTransferItemRequest> Items);

public sealed record DispatchTransferItemRequest(
    Guid StockTransferItemId,
    decimal QuantityToDispatch);

public sealed record DispatchStockTransferRequest(
    string? Notes,
    IReadOnlyCollection<DispatchTransferItemRequest> Items);

public sealed record ReceiveTransferItemRequest(
    Guid StockTransferItemId,
    decimal QuantityReceived,
    decimal QuantityDamaged);

public sealed record ReceiveStockTransferRequest(
    string? Notes,
    IReadOnlyCollection<ReceiveTransferItemRequest> Items);
