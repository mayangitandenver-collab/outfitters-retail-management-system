using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class SaleReturnItem : BaseEntity
{
    public Guid SaleReturnId { get; set; }
    public SaleReturn SaleReturn { get; set; } = null!;
    public Guid SaleItemId { get; set; }
    public SaleItem SaleItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public bool Restock { get; set; } = true;
}
