using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class SupplierReturnItem : BaseEntity
{
    public Guid SupplierReturnId { get; set; }
    public SupplierReturn SupplierReturn { get; set; } = null!;
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}
