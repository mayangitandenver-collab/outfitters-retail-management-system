using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string VariantSku { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
