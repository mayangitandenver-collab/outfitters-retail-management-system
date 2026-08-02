namespace Outfitters.Web.Models;

public sealed class CrmDashboardSummary
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public int AtRiskCustomers { get; set; }
    public int BirthdayCustomersThisMonth { get; set; }
    public decimal TotalLifetimeValue { get; set; }
    public decimal AverageCustomerSpend { get; set; }
    public decimal LoyaltyPointsOutstanding { get; set; }

    public IReadOnlyCollection<TopCustomerItem> TopCustomers { get; set; } =
        Array.Empty<TopCustomerItem>();

    public IReadOnlyCollection<CustomerSegmentItem> Segments { get; set; } =
        Array.Empty<CustomerSegmentItem>();
}

public sealed class TopCustomerItem
{
    public Guid CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TierName { get; set; } = string.Empty;
    public decimal LifetimeSpend { get; set; }
    public decimal LoyaltyPointsBalance { get; set; }
    public decimal StoreCreditBalance { get; set; }
    public int PurchaseCount { get; set; }
    public DateTime? LastPurchaseAtUtc { get; set; }
}

public sealed class CustomerSegmentItem
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal AverageSpend { get; set; }
}

public sealed class LoyaltyAdjustmentRequest
{
    public Guid CustomerId { get; set; }
    public decimal PointsChange { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class StoreCreditAdjustmentRequest
{
    public Guid CustomerId { get; set; }
    public decimal AmountChange { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class CustomerVoucherRequest
{
    public Guid CustomerId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal MinimumSpend { get; set; }
    public DateTime ValidUntilUtc { get; set; }
}
