namespace Outfitters.Web.Models;

public sealed class StockTransferRequest
{
    public Guid SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public IReadOnlyCollection<StockTransferLineRequest> Lines { get; set; } =
        Array.Empty<StockTransferLineRequest>();
}

public sealed class StockTransferLineRequest
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class GiftCardLookupResult
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SalesAnalyticsSummary
{
    public decimal GrossSales { get; set; }
    public decimal NetSales { get; set; }
    public decimal Discounts { get; set; }
    public decimal Returns { get; set; }
    public decimal AverageBasketValue { get; set; }
    public int TransactionCount { get; set; }
}
