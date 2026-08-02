using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class BarcodeAlias : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = "CODE128";
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}
