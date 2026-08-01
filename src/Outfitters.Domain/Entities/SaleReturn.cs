using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class SaleReturn : BaseEntity
{
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid ProcessedByUserId { get; set; }
    public ApplicationUser ProcessedByUser { get; set; } = null!;
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}
