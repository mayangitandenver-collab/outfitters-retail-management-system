namespace Outfitters.Application.Reporting;

public sealed record ReportingDateRange(
    DateOnly StartDate,
    DateOnly EndDate);

public sealed record MonthlySalesPoint(
    int Year,
    int Month,
    string MonthName,
    decimal GrossSales,
    decimal Discounts,
    decimal Taxes,
    decimal NetSales,
    decimal UnitsSold,
    int Transactions);

public sealed record StoreSalesPoint(
    Guid StoreId,
    string StoreCode,
    string StoreName,
    decimal GrossSales,
    decimal Discounts,
    decimal Taxes,
    decimal NetSales,
    decimal UnitsSold,
    int Transactions,
    decimal AverageTransactionValue);

public sealed record InventoryValuationPoint(
    Guid StoreId,
    string StoreCode,
    string StoreName,
    decimal QuantityOnHand,
    decimal CostValue,
    decimal RetailValue,
    decimal PotentialMargin);

public sealed record LowStockPoint(
    Guid StoreId,
    string StoreName,
    Guid ProductVariantId,
    string ProductName,
    string VariantSku,
    string Barcode,
    string? Size,
    string? Color,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal ReorderPoint,
    decimal SuggestedOrderQuantity);
