namespace Outfitters.Web.Models;

public sealed class InventoryIntelligenceSummary
{
    public decimal TotalInventoryValue { get; set; }
    public int LowStockItemCount { get; set; }
    public int OutOfStockItemCount { get; set; }
    public int DeadStockItemCount { get; set; }
    public int ReorderSuggestionCount { get; set; }
    public decimal EstimatedThirtyDayDemand { get; set; }

    public IReadOnlyCollection<InventoryInsightItem> LowStockItems { get; set; } =
        Array.Empty<InventoryInsightItem>();

    public IReadOnlyCollection<InventoryInsightItem> FastMovingItems { get; set; } =
        Array.Empty<InventoryInsightItem>();

    public IReadOnlyCollection<InventoryInsightItem> DeadStockItems { get; set; } =
        Array.Empty<InventoryInsightItem>();

    public IReadOnlyCollection<ReorderSuggestionItem> ReorderSuggestions { get; set; } =
        Array.Empty<ReorderSuggestionItem>();

    public IReadOnlyCollection<BranchInventoryComparisonItem> BranchComparison { get; set; } =
        Array.Empty<BranchInventoryComparisonItem>();
}

public sealed class InventoryInsightItem
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal UnitsSoldThirtyDays { get; set; }
    public decimal DaysOfCover { get; set; }
    public string Classification { get; set; } = "C";
}

public sealed class ReorderSuggestionItem
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal SuggestedOrderQuantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal EstimatedOrderValue =>
        SuggestedOrderQuantity * EstimatedUnitCost;
}

public sealed class BranchInventoryComparisonItem
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal InventoryValue { get; set; }
    public int ActiveSkuCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal ThirtyDaySellThroughRate { get; set; }
}

public sealed class CreatePurchaseRecommendationRequest
{
    public Guid StoreId { get; set; }
    public IReadOnlyCollection<CreatePurchaseRecommendationLine> Lines { get; set; } =
        Array.Empty<CreatePurchaseRecommendationLine>();
}

public sealed class CreatePurchaseRecommendationLine
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
}
