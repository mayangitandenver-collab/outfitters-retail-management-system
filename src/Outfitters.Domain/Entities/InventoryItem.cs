using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class InventoryItem : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public decimal QuantityOnHand { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
}
