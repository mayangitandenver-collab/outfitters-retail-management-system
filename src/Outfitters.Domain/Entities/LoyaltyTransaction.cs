using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public LoyaltyTransactionType Type { get; set; }
    public decimal PointsChange { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
