using Outfitters.Domain.Enums;

namespace Outfitters.Application.Inventory;

public sealed record AdjustInventoryRequest(
    Guid StoreId,
    Guid ProductVariantId,
    decimal QuantityChange,
    InventoryTransactionType TransactionType,
    string? ReferenceNumber,
    string? Remarks);
