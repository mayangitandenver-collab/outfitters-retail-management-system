using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class CustomerTier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal MinimumLifetimeSpend { get; set; }
    public decimal PointsMultiplier { get; set; } = 1m;
    public decimal DefaultDiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
