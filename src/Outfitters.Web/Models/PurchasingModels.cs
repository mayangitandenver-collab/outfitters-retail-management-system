namespace Outfitters.Web.Models;

public sealed class PurchasingDashboardSummary
{
    public int ActiveSupplierCount { get; set; }
    public int DraftPurchaseOrderCount { get; set; }
    public int AwaitingApprovalCount { get; set; }
    public int OverdueDeliveryCount { get; set; }
    public decimal OpenPurchaseOrderValue { get; set; }
    public decimal ReceivedThisMonthValue { get; set; }

    public IReadOnlyCollection<PurchaseOrderListItem> PurchaseOrders { get; set; } =
        Array.Empty<PurchaseOrderListItem>();

    public IReadOnlyCollection<SupplierPerformanceItem> Suppliers { get; set; } =
        Array.Empty<SupplierPerformanceItem>();
}

public sealed class PurchaseOrderListItem
{
    public Guid Id { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
}

public sealed class SupplierPerformanceItem
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal OnTimeDeliveryRate { get; set; }
    public decimal OrderAccuracyRate { get; set; }
    public decimal ReturnRate { get; set; }
    public decimal AverageLeadTimeDays { get; set; }
    public decimal TotalPurchaseValue { get; set; }
}

public sealed class CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; set; }
    public Guid StoreId { get; set; }
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public string Notes { get; set; } = string.Empty;

    public IReadOnlyCollection<CreatePurchaseOrderLineRequest> Lines { get; set; } =
        Array.Empty<CreatePurchaseOrderLineRequest>();
}

public sealed class CreatePurchaseOrderLineRequest
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class GoodsReceiptRequest
{
    public Guid PurchaseOrderId { get; set; }
    public string DeliveryReceiptNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public IReadOnlyCollection<GoodsReceiptLineRequest> Lines { get; set; } =
        Array.Empty<GoodsReceiptLineRequest>();
}

public sealed class GoodsReceiptLineRequest
{
    public Guid ProductVariantId { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityDamaged { get; set; }
}
