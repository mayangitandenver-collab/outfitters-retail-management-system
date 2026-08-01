using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class SupplierReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid ProcessedByUserId { get; set; }
    public ApplicationUser ProcessedByUser { get; set; } = null!;
    public DateTime ReturnDateUtc { get; set; } = DateTime.UtcNow;
    public SupplierReturnStatus Status { get; set; } = SupplierReturnStatus.Completed;
    public decimal TotalCost { get; set; }
    public string? Reason { get; set; }
    public ICollection<SupplierReturnItem> Items { get; set; } = new List<SupplierReturnItem>();
}
