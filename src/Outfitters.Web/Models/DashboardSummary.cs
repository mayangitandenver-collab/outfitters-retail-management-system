namespace Outfitters.Web.Models;

public sealed class DashboardSummary
{
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public decimal InventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public int ActiveCustomers { get; set; }
    public int TransactionsToday { get; set; }
    public IReadOnlyCollection<DashboardMetric> SalesTrend { get; set; } =
        Array.Empty<DashboardMetric>();
    public IReadOnlyCollection<TopProductItem> TopProducts { get; set; } =
        Array.Empty<TopProductItem>();
}

public sealed class DashboardMetric
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class TopProductItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal SalesAmount { get; set; }
}
