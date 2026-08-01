using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class CashSession : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid OpenedByUserId { get; set; }
    public ApplicationUser OpenedByUser { get; set; } = null!;
    public Guid? ClosedByUserId { get; set; }
    public ApplicationUser? ClosedByUser { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? CashVariance { get; set; }
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
