namespace Outfitters.Application.Purchasing;

public sealed record CreateSupplierRequest(
    string Code,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxIdentificationNumber,
    int PaymentTermsDays);

public sealed record PurchaseOrderItemRequest(
    Guid ProductVariantId,
    decimal OrderedQuantity,
    decimal UnitCost,
    decimal DiscountAmount,
    decimal TaxAmount);

public sealed record CreatePurchaseOrderRequest(
    Guid SupplierId,
    Guid StoreId,
    DateTime? ExpectedDeliveryDateUtc,
    string? Notes,
    IReadOnlyCollection<PurchaseOrderItemRequest> Items);

public sealed record ReceivePurchaseOrderItemRequest(
    Guid PurchaseOrderItemId,
    decimal QuantityReceived,
    decimal UnitCost);

public sealed record ReceivePurchaseOrderRequest(
    string? SupplierInvoiceNumber,
    string? Notes,
    IReadOnlyCollection<ReceivePurchaseOrderItemRequest> Items);

public sealed record SupplierReturnItemRequest(
    Guid ProductVariantId,
    decimal Quantity,
    decimal UnitCost);

public sealed record CreateSupplierReturnRequest(
    Guid SupplierId,
    Guid StoreId,
    string? Reason,
    IReadOnlyCollection<SupplierReturnItemRequest> Items);
